using System;
namespace GymApp.Models;

public class SupplementIntake
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SupplementId { get; set; }
    public double Dosage { get; set; }
    public DayOfWeek Date { get; set; }
    public TimeSpan Time { get; set; }

    public User User { get; set; }
    public Supplement Supplement { get; set; }
}