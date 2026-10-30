using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Services;
using GymApp.Validators;

namespace GymApp.ViewModels;

public partial class NutritionViewModel : ViewModelBase
{
    public List<string> GenderOptions { get; } = new() { "Male", "Female" };
    public List<string> ActivityLevelOptions { get; } = new() { "Sedentary", "Light", "Moderate", "Active", "VeryActive" };
    public List<string> GoalOptions { get; } = new() { "Lose", "Maintain", "Gain" };
    
    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool profileDataConsentGranted;
    
    [ObservableProperty] 
    private int weight;
    [ObservableProperty]
    private int height;
    [ObservableProperty] 
    private int age;
    [ObservableProperty]
    private string gender = "Male";
    [ObservableProperty]
    private string activityLevel = "Sedentary";
    [ObservableProperty]
    private string goal = "Maintain";

    [ObservableProperty] 
    private double bmr;
    [ObservableProperty] 
    private double tdee;
    [ObservableProperty] 
    private double calories;
    [ObservableProperty] 
    private double protein;
    [ObservableProperty] 
    private double fat;
    [ObservableProperty] 
    private double carbs;
    
    private readonly NutritionValidator _validator = new();

    public void SetProfileDataConsent(bool granted)
    {
        ProfileDataConsentGranted = granted;
        if (granted) return;

        Weight = 0;
        Height = 0;
        Age = 0;
        Gender = "Male";
        ClearResults();
        ValidationMessage = "";
    }
    
    [RelayCommand]
    private void Calculate()
    {
        if (!ProfileDataConsentGranted)
        {
            ClearResults();
            ValidationMessage = "Give consent for optional profile data in Profile before using this calculator.";
            return;
        }

        var result = _validator.Validate(this);
    
        if (!result.IsValid)
        {
            ClearResults();
            ValidationMessage = result.Errors[0].ErrorMessage;
            return;
        }
    
        ValidationMessage = "";
        
        double calculatedBmr = NutritionCalculator.CalculateBmr(weight, height, age, gender);
        double calculatedTdee = NutritionCalculator.CalculateTdee(calculatedBmr, activityLevel);
        double calculatedCalories = NutritionCalculator.AdjustCaloriesForGoal(calculatedTdee, goal);
        var (calculatedProtein, calculatedFat, calculatedCarbs) = NutritionCalculator.CalculateMacros(calculatedCalories, weight);

        if (!double.IsFinite(calculatedCalories) || calculatedCalories <= 1000)
        {
            ClearResults();
            ValidationMessage = "This estimate is 1,000 kcal/day or lower, which can require professional supervision. Try Maintain or ask a dietitian for personal guidance.";
            return;
        }

        if (calculatedProtein * 4 + calculatedFat * 9 > calculatedCalories)
        {
            ClearResults();
            ValidationMessage = "The default protein and fat estimates exceed this calorie target. Try Maintain or ask a dietitian for personal guidance.";
            return;
        }

        Bmr = Math.Round(calculatedBmr, 1);
        Tdee = Math.Round(calculatedTdee, 1);
        Calories = Math.Round(calculatedCalories, 1);
        Protein = Math.Round(calculatedProtein, 1);
        Fat = Math.Round(calculatedFat, 1);
        Carbs = Math.Round(calculatedCarbs, 1);
    }

    private void ClearResults()
    {
        Bmr = 0;
        Tdee = 0;
        Calories = 0;
        Protein = 0;
        Fat = 0;
        Carbs = 0;
    }
}
