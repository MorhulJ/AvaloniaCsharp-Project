using System;
namespace GymApp.Models;

public class PersonalRecord
{
    public string FirebaseId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string ExerciseFirebaseId { get; set; } = "";
    public double Value { get; set; }
    public DateTimeOffset Date { get; set; }

    public User? User { get; set; }
    public Exercise? Exercise { get; set; }
}