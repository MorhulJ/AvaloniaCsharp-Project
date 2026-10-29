using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Auth;
using Google.Cloud.Firestore;

namespace GymApp.Services;

public class AuthService
{
    private static UserCredential? _currentCredential;
    private static string? _currentUserId;
    private static string? _currentEmail;

    public string? CurrentUserId => _currentUserId;
    public string? CurrentEmail => _currentEmail;
    public string? LastErrorMessage { get; private set; }

    public AuthService()
    {
        FirebaseService.RefreshTokenUpdated += PersistRotatedRefreshToken;
    }

    public async Task<bool> RegisterAsync(string email, string password, string name)
    {
        LastErrorMessage = null;
        _currentCredential = null;
        _currentUserId = null;
        _currentEmail = null;
        FirebaseService.ClearSession();
        var stage = "create the Firebase account";
        try
        {
            FirebaseService.Initialize();
            var auth = FirebaseService.GetAuth();
            var credential = await auth.CreateUserWithEmailAndPasswordAsync(email, password, name);

            _currentCredential = credential;
            _currentUserId = credential.User.Uid;
            _currentEmail = email;
            FirebaseService.SetSession(credential);

            stage = "save the account profile";
            var db = FirebaseService.GetDb();
            await db.Collection("users").Document(_currentUserId).SetAsync(new
            {
                name = name,
                email = email,
                gender = "Male",
                weight = 0,
                height = 0,
                dateOfBirth = DateTime.Now.ToString("yyyy-MM-dd")
            });

            stage = "add the starter workout data";
            await SeedUserDataAsync(_currentUserId);

            return true;
        }
        catch (FirebaseAuthException ex)
        {
            LastErrorMessage = GetAuthErrorMessage(ex.Reason);
            return false;
        }
        catch
        {
            LastErrorMessage = _currentUserId is not null && stage != "create the Firebase account"
                ? $"Your account was created, but Firebase couldn't {stage}. Sign in with this account instead of registering again."
                : "Couldn't connect to Firebase to create the account. Check your connection and try again.";
            return false;
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        LastErrorMessage = null;
        try
        {
            FirebaseService.Initialize();
            var auth = FirebaseService.GetAuth();
            var credential = await auth.SignInWithEmailAndPasswordAsync(email, password);

            _currentCredential = credential;
            _currentUserId = credential.User.Uid;
            _currentEmail = email;
            FirebaseService.SetSession(credential);

            return true;
        }
        catch (FirebaseAuthException ex)
        {
            LastErrorMessage = GetAuthErrorMessage(ex.Reason);
            return false;
        }
        catch
        {
            LastErrorMessage = "Couldn't connect to Firebase. Check your connection and try again.";
            return false;
        }
    }

    private static string GetAuthErrorMessage(AuthErrorReason reason) => reason switch
    {
        AuthErrorReason.EmailExists => "An account with this email already exists. Sign in instead.",
        AuthErrorReason.InvalidEmailAddress => "Enter a valid email address.",
        AuthErrorReason.WeakPassword => "Choose a stronger password (at least 6 characters).",
        AuthErrorReason.UnknownEmailAddress or AuthErrorReason.UserNotFound or AuthErrorReason.WrongPassword
            => "Email or password is incorrect.",
        AuthErrorReason.UserDisabled => "This account is disabled. Contact support.",
        AuthErrorReason.TooManyAttemptsTryLater => "Too many attempts. Wait a while, then try again.",
        AuthErrorReason.OperationNotAllowed => "Email and password sign-in is disabled in Firebase.",
        AuthErrorReason.InvalidApiKey => "Firebase configuration is invalid. Contact the app developer.",
        _ => "Firebase couldn't complete this request. Please try again."
    };

    public void Logout()
    {
        _currentCredential = null;
        _currentUserId = null;
        _currentEmail = null;
        FirebaseService.ClearSession();
        DeleteRememberMe();
    }

    public async Task ReauthenticateAsync(string email, string password)
    {
        FirebaseService.Initialize();
        var credential = await FirebaseService.GetAuth()
            .SignInWithEmailAndPasswordAsync(email, password);
        _currentCredential = credential;
        _currentUserId = credential.User.Uid;
        _currentEmail = email;
        FirebaseService.SetSession(credential);
        if (ProtectedSessionStore.Exists && !string.IsNullOrWhiteSpace(credential.User.Credential.RefreshToken))
            ProtectedSessionStore.Save(credential.User.Credential.RefreshToken, email);
    }

    public async Task DeleteCurrentAccountAsync()
    {
        await FirebaseService.DeleteCurrentAccountAsync();
        _currentCredential = null;
        _currentUserId = null;
        _currentEmail = null;
        FirebaseService.ClearSession();
        DeleteRememberMe();
    }

    public bool SaveRememberMe(bool rememberMe)
    {
        if (!rememberMe)
        {
            ProtectedSessionStore.Delete();
            return true;
        }

        try
        {
            var refreshToken = FirebaseService.GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken)) return false;
            ProtectedSessionStore.Save(refreshToken, _currentEmail ?? "");
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> TryAutoLoginAsync()
    {
        // Discard the old insecure UID-only marker from previous app versions.
        var legacyPath = GetRememberMePath();
        if (File.Exists(legacyPath)) File.Delete(legacyPath);
        if (!ProtectedSessionStore.Exists) return false;

        try
        {
            var session = ProtectedSessionStore.Load();
            if (string.IsNullOrWhiteSpace(session?.RefreshToken)) return false;
            _currentUserId = await FirebaseService.RestoreSessionAsync(session.RefreshToken);
            _currentEmail = session.Email;
            return true;
        }
        catch (Exception ex)
        {
            FirebaseService.ClearSession();
            _currentUserId = null;
            _currentEmail = null;

            if (ex is HttpRequestException or TaskCanceledException)
            {
                LastErrorMessage = "Couldn't check your saved sign-in because GymApp couldn't reach Firebase. Check your internet connection and try again. Your saved sign-in was kept.";
                return false;
            }

            ProtectedSessionStore.Delete();
            LastErrorMessage = "Your saved sign-in has expired. Sign in again to continue.";
            return false;
        }
    }

    private static void PersistRotatedRefreshToken(string refreshToken)
    {
        try
        {
            if (ProtectedSessionStore.Exists)
            {
                var session = ProtectedSessionStore.Load();
                ProtectedSessionStore.Save(refreshToken, session?.Email ?? _currentEmail ?? "");
            }
        }
        catch { ProtectedSessionStore.Delete(); }
    }

    public void DeleteRememberMe()
    {
        ProtectedSessionStore.Delete();
        var path = GetRememberMePath();
        if (File.Exists(path))
            File.Delete(path);
    }

    private string GetRememberMePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GymApp",
            "remember.txt"
        );
    }

    private async Task SeedUserDataAsync(string userId)
    {
        var db = FirebaseService.GetDb();

        var exercises = new[]
        {
            new { name = "Bench Press", muscleGroup = "Chest", description = "Barbell press on flat bench" },
            new { name = "Squat", muscleGroup = "Legs", description = "Barbell squat" },
            new { name = "Deadlift", muscleGroup = "Back", description = "Conventional deadlift" },
            new { name = "Pull-up", muscleGroup = "Back", description = "Bodyweight pull-up" },
            new { name = "Overhead Press", muscleGroup = "Shoulders", description = "Standing barbell press" },
            new { name = "Barbell Row", muscleGroup = "Back", description = "Bent-over barbell row" },
            new { name = "Dumbbell Curl", muscleGroup = "Biceps", description = "Standing dumbbell curl" },
            new { name = "Tricep Pushdown", muscleGroup = "Triceps", description = "Cable pushdown" },
            new { name = "Leg Press", muscleGroup = "Legs", description = "Machine leg press" },
            new { name = "Plank", muscleGroup = "Core", description = "Static plank hold" }
        };

        var exerciseIds = new List<string>();

        foreach (var exercise in exercises)
        {
            var docRef = await db.Collection("users").Document(userId)
                .Collection("exercises").AddAsync(exercise);
            exerciseIds.Add(docRef.Id);
        }

        var supplements = new[]
        {
            new { name = "Creatine", dosageUnit = "g", description = "Increases strength and muscle mass" },
            new { name = "Whey Protein", dosageUnit = "g", description = "Fast protein for muscle recovery" },
            new { name = "BCAA", dosageUnit = "g", description = "Amino acids for muscle protection" },
            new { name = "Vitamin D", dosageUnit = "mg", description = "Supports bone health and immunity" },
            new { name = "Omega-3", dosageUnit = "mg", description = "Supports heart and joint health" }
        };

        foreach (var supplement in supplements)
        {
            await db.Collection("users").Document(userId)
                .Collection("supplements").AddAsync(supplement);
        }

        var programRef = await db.Collection("users").Document(userId)
            .Collection("workoutPrograms").AddAsync(new
            {
                name = "Fullbody",
                description = "Full body workout 3 times per week",
                createdDate = DateTime.Now.ToString("yyyy-MM-dd")
            });

        var programExercises = new[]
        {
            new { exerciseId = exerciseIds[1], sets = 4, reps = 8, restTime = 120, orderIndex = 1 },
            new { exerciseId = exerciseIds[0], sets = 4, reps = 8, restTime = 90, orderIndex = 2 },
            new { exerciseId = exerciseIds[2], sets = 3, reps = 6, restTime = 150, orderIndex = 3 },
            new { exerciseId = exerciseIds[3], sets = 3, reps = 10, restTime = 90, orderIndex = 4 },
            new { exerciseId = exerciseIds[4], sets = 3, reps = 10, restTime = 90, orderIndex = 5 }
        };

        foreach (var pe in programExercises)
        {
            await db.Collection("users").Document(userId)
                .Collection("workoutPrograms").Document(programRef.Id)
                .Collection("exercises").AddAsync(pe);
        }
    }
}
