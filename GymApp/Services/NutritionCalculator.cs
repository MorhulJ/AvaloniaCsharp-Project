using System;
namespace GymApp.Services;

public static class NutritionCalculator
{
    public static double CalculateBmr(int weight, int height, int age, string gender)
    {
        double bmr = 10 * weight + 6.25 * height - 5 * age;
        bmr += gender.ToLower() == "male" ? 5 : -161;
        return bmr;
    }

    public static double CalculateTdee(double bmr, string activityLevel)
    {
        double multiplier;
        
        switch (activityLevel)
        {
            case "Sedentary":
                multiplier = 1.2;
                break;
            case "Light":
                multiplier = 1.375;
                break;
            case "Moderate":
                multiplier = 1.55;
                break;
            case "Active":
                multiplier = 1.725;
                break;
            case "VeryActive":
                multiplier = 1.9;
                break;
            default:
                multiplier = 1.2;
                break;
        }
        return bmr * multiplier;
    }

    public static double AdjustCaloriesForGoal(double tdee, string goal)
    {
        switch (goal)
        {
            case "Lose":
                return tdee - 500;
            case "Gain":
                return tdee + 500;
            default:
                return tdee;
        }
    }

    public static (double protein, double fat, double carbs) CalculateMacros(double calories, int weightKg)
    {
        // Білок: 2г/кг, Жир: 1г/кг, решта - вуглеводи
        double protein = weightKg * 2;
        double fat = weightKg * 1;
        double remainingCalories = calories - (protein * 4) - (fat * 9);
        double carbs = remainingCalories / 4;

        return (protein, fat, Math.Max(carbs, 0));
    }
}