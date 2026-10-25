using System;
using Firebase.Auth;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Firebase.Auth.Providers;
using Google.Cloud.Firestore;

namespace GymApp.Services;

public class FirebaseService
{
    private static readonly HttpClient Http = new();
    private static FirebaseAuthClient? _authClient;
    private static FirestoreDb? _db;
    private static string? _idToken;
    private static string? _refreshToken;
    private static DateTimeOffset _tokenExpiresAt;
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
        if (DateTimeOffset.UtcNow < _tokenExpiresAt && !string.IsNullOrWhiteSpace(_idToken))
            return _idToken;
        if (string.IsNullOrWhiteSpace(_refreshToken))
            throw new InvalidOperationException("Sign in is required to access Firestore.");

        await RefreshTokensAsync(_refreshToken);
        return _idToken!;
    }

    public static async Task<string> RestoreSessionAsync(string refreshToken)
    {
        await RefreshTokensAsync(refreshToken);
        return _uid ?? throw new InvalidOperationException("Firebase did not return the account ID.");
    }

    private static string? _uid;

    private static async Task RefreshTokensAsync(string refreshToken)
    {
        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });
        using var response = await Http.PostAsync(
            "https://securetoken.googleapis.com/v1/token?key=AIzaSyCk8jFOyXPO92qmvz_KDATgEr5jVj3u1Jw",
            content);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Your Firebase session expired. Please sign in again.");

        using var json = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        _idToken = json.RootElement.GetProperty("id_token").GetString();
        _refreshToken = json.RootElement.GetProperty("refresh_token").GetString();
        if (json.RootElement.TryGetProperty("user_id", out var userId)) _uid = userId.GetString();
        var expiresIn = json.RootElement.TryGetProperty("expires_in", out var expires)
            && int.TryParse(expires.GetString(), out var seconds) ? seconds : 3600;
        _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, expiresIn - 60));
        if (!string.IsNullOrEmpty(_refreshToken)) RefreshTokenUpdated?.Invoke(_refreshToken);
    }

    public static void SetSession(UserCredential credential)
    {
        _idToken = credential.User.Credential.IdToken;
        _refreshToken = credential.User.Credential.RefreshToken;
        _uid = credential.User.Uid;
        _tokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(50);
    }

    public static string? GetRefreshToken() => _refreshToken;

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
        _idToken = null;
        _refreshToken = null;
        _uid = null;
        _tokenExpiresAt = DateTimeOffset.MinValue;
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
