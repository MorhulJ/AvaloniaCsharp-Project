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
    
    [RelayCommand]
    private void Calculate()
    {
        var result = _validator.Validate(this);
    
        if (!result.IsValid)
        {
            ValidationMessage = result.Errors[0].ErrorMessage;
            return;
        }
    
        ValidationMessage = "";
        
        double calculatedBmr = NutritionCalculator.CalculateBmr(weight, height, age, gender);
        double calculatedTdee = NutritionCalculator.CalculateTdee(calculatedBmr, activityLevel);
        double calculatedCalories = NutritionCalculator.AdjustCaloriesForGoal(calculatedTdee, goal);
        var (calculatedProtein, calculatedFat, calculatedCarbs) = NutritionCalculator.CalculateMacros(calculatedCalories, weight);

        Bmr = Math.Round(calculatedBmr, 1);
        Tdee = Math.Round(calculatedTdee, 1);
        Calories = Math.Round(calculatedCalories, 1);
        Protein = Math.Round(calculatedProtein, 1);
        Fat = Math.Round(calculatedFat, 1);
        Carbs = Math.Round(calculatedCarbs, 1);
    }
}