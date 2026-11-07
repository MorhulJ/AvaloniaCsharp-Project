using System;
using Firebase.Auth;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Firebase.Auth.Providers;
using Google.Cloud.Firestore;

namespace GymApp.Services;

internal sealed class SavedSessionRejectedException()
    : InvalidOperationException("Your Firebase session expired. Please sign in again.");

public class FirebaseService
{
    private static readonly HttpClient Http = new();
    private static FirebaseAuthClient? _authClient;
    private static FirestoreDb? _db;
    private static string? _idToken;
    private static string? _refreshToken;
    private static DateTimeOffset _tokenExpiresAt;
    private static readonly object SessionLock = new();
    private static readonly SemaphoreSlim RefreshGate = new(1);
    private static long _sessionVersion;
    public static event Action<string>? RefreshTokenUpdated;

    public static void Initialize()
    {
        if (_authClient is not null) return;

        var config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyCk8jFOyXPO92qmvz_KDATgEr5jVj3u1Jw",
            AuthDomain = "gymapp-9c7e4.firebaseapp.com",
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()
            }
        };

        _authClient = new FirebaseAuthClient(config);
        _db = FirestoreDb.Create("gymapp-9c7e4", GetIdTokenAsync);
    }

    public static async Task<string> GetIdTokenAsync()
    {
        long version;
        lock (SessionLock)
        {
            if (DateTimeOffset.UtcNow < _tokenExpiresAt && !string.IsNullOrWhiteSpace(_idToken))
                return _idToken;
            version = _sessionVersion;
        }
        return (await RefreshTokensAsync(version, null)).IdToken;
    }

    public static async Task<string> RestoreSessionAsync(string refreshToken)
    {
        long version;
        lock (SessionLock) version = _sessionVersion;
        return (await RefreshTokensAsync(version, refreshToken)).UserId;
    }

    private static string? _uid;

    private static async Task<(string IdToken, string UserId)> RefreshTokensAsync(long version, string? savedToken)
    {
        await RefreshGate.WaitAsync();
        try
        {
            string refreshToken;
            lock (SessionLock)
            {
                CheckSessionVersion(version);
                if (savedToken is null && _idToken is not null && _uid is not null && DateTimeOffset.UtcNow < _tokenExpiresAt)
                    return (_idToken, _uid);
                refreshToken = savedToken ?? _refreshToken
                    ?? throw new InvalidOperationException("Sign in is required to access Firestore.");
            }

            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });
            using var response = await Http.PostAsync(
                "https://securetoken.googleapis.com/v1/token?key=AIzaSyCk8jFOyXPO92qmvz_KDATgEr5jVj3u1Jw",
                content);
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
                throw new SavedSessionRejectedException();
            response.EnsureSuccessStatusCode();

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var idToken = json.RootElement.GetProperty("id_token").GetString()
                ?? throw new InvalidOperationException("Firebase did not return an ID token.");
            var userId = json.RootElement.GetProperty("user_id").GetString()
                ?? throw new InvalidOperationException("Firebase did not return the account ID.");
            var rotatedToken = json.RootElement.GetProperty("refresh_token").GetString() ?? refreshToken;
            var expiresIn = json.RootElement.TryGetProperty("expires_in", out var expires)
                && int.TryParse(expires.GetString(), out var seconds) ? seconds : 3600;

            lock (SessionLock)
            {
                CheckSessionVersion(version);
                _idToken = idToken;
                _uid = userId;
                _refreshToken = rotatedToken;
                _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, expiresIn - 60));
                if (rotatedToken != refreshToken)
                    RefreshTokenUpdated?.Invoke(rotatedToken);
            }
            return (idToken, userId);
        }
        finally { RefreshGate.Release(); }
    }

    private static void CheckSessionVersion(long version)
    {
        if (version != _sessionVersion)
            throw new InvalidOperationException("The sign-in session changed. Please retry.");
    }

    public static void SetSession(UserCredential credential)
    {
        lock (SessionLock)
        {
            _sessionVersion++;
            _idToken = credential.User.Credential.IdToken;
            _refreshToken = credential.User.Credential.RefreshToken;
            _uid = credential.User.Uid;
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(50);
        }
    }

    public static string? GetRefreshToken()
    {
        lock (SessionLock) return _refreshToken;
    }

    public static async Task DeleteCurrentAccountAsync()
    {
        var idToken = await GetIdTokenAsync();
        var payload = JsonSerializer.Serialize(new { idToken });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await Http.PostAsync(
            "https://identitytoolkit.googleapis.com/v1/accounts:delete?key=AIzaSyCk8jFOyXPO92qmvz_KDATgEr5jVj3u1Jw",
            content);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            var reason = body;
            try
            {
                using var error = JsonDocument.Parse(body);
                if (error.RootElement.TryGetProperty("error", out var firebaseError)
                    && firebaseError.TryGetProperty("message", out var message))
                    reason = message.GetString() ?? body;
            }
            catch (JsonException) { }

            throw new InvalidOperationException($"Firebase rejected account deletion (HTTP {(int)response.StatusCode}): {reason}");
        }
    }

    public static void ClearSession()
    {
        lock (SessionLock)
        {
            _sessionVersion++;
            _idToken = null;
            _refreshToken = null;
            _uid = null;
            _tokenExpiresAt = DateTimeOffset.MinValue;
        }
    }

    public static FirestoreDb GetDb()
    {
        return _db ?? throw new InvalidOperationException("Firebase not initialized");
    }

    public static FirebaseAuthClient GetAuth()
    {
        if (_authClient == null)
            throw new InvalidOperationException("Firebase not initialized");
        return _authClient;
    }
}
