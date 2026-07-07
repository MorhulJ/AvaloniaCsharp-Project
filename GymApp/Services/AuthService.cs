using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private static User? _currentUser;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public User? CurrentUser => _currentUser;

    public string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public async Task<User?> LoginAsync(string login, string password)
    {
        var hash = HashPassword(password);
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Login == login && u.PasswordHash == hash);

        if (user != null)
            _currentUser = user;

        return user;
    }

    public async Task<bool> RegisterAsync(string login, string password, string name)
{
    var exists = await _db.Users.AnyAsync(u => u.Login == login);
    if (exists) return false;

    var user = new User
    {
        Login = login,
        PasswordHash = HashPassword(password),
        Name = name,
        DateOfBirth = DateTime.Now,
    };

    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    _currentUser = user;

    await SeedUserDataAsync(user.Id);

    return true;
}

private async Task SeedUserDataAsync(int userId)
{
    var exercises = new List<Exercise>
    {
        new() { Name = "Bench Press", MuscleGroup = "Chest", Description = "Barbell press on flat bench" },
        new() { Name = "Squat", MuscleGroup = "Legs", Description = "Barbell squat" },
        new() { Name = "Deadlift", MuscleGroup = "Back", Description = "Conventional deadlift" },
        new() { Name = "Pull-up", MuscleGroup = "Back", Description = "Bodyweight pull-up" },
        new() { Name = "Overhead Press", MuscleGroup = "Shoulders", Description = "Standing barbell press" },
        new() { Name = "Barbell Row", MuscleGroup = "Back", Description = "Bent-over barbell row" },
        new() { Name = "Dumbbell Curl", MuscleGroup = "Biceps", Description = "Standing dumbbell curl" },
        new() { Name = "Tricep Pushdown", MuscleGroup = "Triceps", Description = "Cable pushdown" },
        new() { Name = "Leg Press", MuscleGroup = "Legs", Description = "Machine leg press" },
        new() { Name = "Plank", MuscleGroup = "Core", Description = "Static plank hold" }
    };

    _db.Exercises.AddRange(exercises);
    await _db.SaveChangesAsync();
    
    var supplements = new List<Supplement>
    {
        new() { Name = "Creatine", DosageUnit = "g", Description = "Increases strength and muscle mass" },
        new() { Name = "Whey Protein", DosageUnit = "g", Description = "Fast protein for muscle recovery" },
        new() { Name = "BCAA", DosageUnit = "g", Description = "Amino acids for muscle protection" },
        new() { Name = "Vitamin D", DosageUnit = "mg", Description = "Supports bone health and immunity" },
        new() { Name = "Omega-3", DosageUnit = "mg", Description = "Supports heart and joint health" }
    };

    _db.Supplements.AddRange(supplements);
    await _db.SaveChangesAsync();
    
    var fullbody = new WorkoutProgram
    {
        UserId = userId,
        Name = "Fullbody",
        Description = "Full body workout 3 times per week",
        CreatedDate = DateTime.Now
    };

    _db.WorkoutPrograms.Add(fullbody);
    await _db.SaveChangesAsync();
    
    var benchPress = exercises.First(e => e.Name == "Bench Press");
    var squat = exercises.First(e => e.Name == "Squat");
    var deadlift = exercises.First(e => e.Name == "Deadlift");
    var pullUp = exercises.First(e => e.Name == "Pull-up");
    var ohp = exercises.First(e => e.Name == "Overhead Press");

    var programExercises = new List<ProgramExercise>
    {
        new() { ProgramId = fullbody.Id, ExerciseId = squat.Id, Sets = 4, Reps = 8, RestTime = 120, OrderIndex = 1 },
        new() { ProgramId = fullbody.Id, ExerciseId = benchPress.Id, Sets = 4, Reps = 8, RestTime = 90, OrderIndex = 2 },
        new() { ProgramId = fullbody.Id, ExerciseId = deadlift.Id, Sets = 3, Reps = 6, RestTime = 150, OrderIndex = 3 },
        new() { ProgramId = fullbody.Id, ExerciseId = pullUp.Id, Sets = 3, Reps = 10, RestTime = 90, OrderIndex = 4 },
        new() { ProgramId = fullbody.Id, ExerciseId = ohp.Id, Sets = 3, Reps = 10, RestTime = 90, OrderIndex = 5 }
    };

    _db.WorkoutExercises.AddRange(programExercises);
    await _db.SaveChangesAsync();
}

    public void Logout()
    {
        _currentUser = null;
        DeleteRememberMe();
    }

    public void SaveRememberMe(int userId)
    {
        var path = GetRememberMePath();
        File.WriteAllText(path, userId.ToString());
    }

    public async Task<User?> TryAutoLoginAsync()
    {
        var path = GetRememberMePath();
        if (!File.Exists(path)) return null;

        var content = await File.ReadAllTextAsync(path);
        if (!int.TryParse(content, out var userId)) return null;

        var user = await _db.Users.FindAsync(userId);
        if (user != null)
            _currentUser = user;

        return user;
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
}