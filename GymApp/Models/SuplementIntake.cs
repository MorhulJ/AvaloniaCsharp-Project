using System;
namespace GymApp.Models;

public class SupplementIntake
{
    public string FirebaseId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string SupplementFirebaseId { get; set; } = "";
    public double Dosage { get; set; }
    public DayOfWeek Date { get; set; }
    public TimeSpan Time { get; set; }

    public User? User { get; set; }
    public Supplement? Supplement { get; set; }
}