using System;
namespace GymApp.Models;

public class Goal
{
    public string FirebaseId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string? ExerciseFirebaseId { get; set; }
    public string Title { get; set; } = "";
    public double TargetValue { get; set; }
    public double CurrentValue { get; set; }

    public User? User { get; set; }
    public Exercise? Exercise { get; set; }
}