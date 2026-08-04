using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Google.Cloud.Firestore;

namespace GymApp.Services;

public class AuthService
{
    private static UserCredential? _currentCredential;
    private static string? _currentUserId;

    public string? CurrentUserId => _currentUserId;

    public async Task<bool> RegisterAsync(string email, string password, string name)
    {
        try
        {
            var auth = FirebaseService.GetAuth();
            var credential = await auth.CreateUserWithEmailAndPasswordAsync(email, password, name);

            _currentCredential = credential;
            _currentUserId = credential.User.Uid;

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

            await SeedUserDataAsync(_currentUserId);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var auth = FirebaseService.GetAuth();
            var credential = await auth.SignInWithEmailAndPasswordAsync(email, password);

            _currentCredential = credential;
            _currentUserId = credential.User.Uid;

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Logout()
    {
        _currentCredential = null;
        _currentUserId = null;
        DeleteRememberMe();
    }

    public void SaveRememberMe(string userId)
    {
        var path = GetRememberMePath();
        var directory = Path.GetDirectoryName(path)!;

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, userId);
    }

    public async Task<bool> TryAutoLoginAsync()
    {
        var path = GetRememberMePath();
        if (!File.Exists(path)) return false;

        var userId = await File.ReadAllTextAsync(path);
        if (string.IsNullOrEmpty(userId)) return false;

        _currentUserId = userId;
        return true;
    }

    public void DeleteRememberMe()
    {
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