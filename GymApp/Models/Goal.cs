using System;
namespace GymApp.Models;

public class Goal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ExerciseId { get; set; }
    public string Title { get; set; } = "";
    public double TargetValue { get; set; }
    public string Status { get; set; } = "InProgress";

    public User User { get; set; }
    public Exercise? Exercise { get; set; }
}