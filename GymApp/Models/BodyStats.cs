using System;
namespace GymApp.Models;

public class BodyStats
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public double Weight { get; set; }

    public User User { get; set; }
}