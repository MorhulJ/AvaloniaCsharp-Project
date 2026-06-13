using System;
namespace GymApp.Models;

public class NutritionProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ActivityLevel { get; set; } = ""; // "Sedentary", "Light", "Moderate", "Active", "VeryActive"
    public string Goal { get; set; } = ""; // "Lose", "Maintain", "Gain"
    public double Bmr { get; set; }      // базовий метаболізм
    public double Tdee { get; set; }     // загальна витрата калорій
    public double Calories { get; set; } // фінальна ціль по калоріях
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbs { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.Now;

    public User User { get; set; }
}