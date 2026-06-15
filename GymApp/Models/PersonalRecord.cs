using System;
namespace GymApp.Models;

public class PersonalRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public double Value { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Type { get; set; } = "";

    public User User { get; set; }
    public Exercise Exercise { get; set; }
}