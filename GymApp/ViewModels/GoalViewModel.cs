using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using System.Threading.Tasks;
namespace GymApp.ViewModels;

public partial class GoalViewModel : ViewModelBase
{
    private readonly GoalService _goalService;
    private readonly  ExerciseService _exerciseService;
    
    public ObservableCollection<Goal> Goals { get; } = new();
    
    public ObservableCollection<Exercise> Exercises { get; } = new();

    [ObservableProperty]
    private string goalTitle = "";
    [ObservableProperty]
    private Exercise? goalExercise;
    [ObservableProperty]
    private double goalValue;
    [ObservableProperty]
    private double currentValue;

    public GoalViewModel(GoalService goalService, ExerciseService exerciseService)
    {
        _goalService = goalService;
        _exerciseService = exerciseService;
    }

    [RelayCommand]
    private async Task SaveGoalAsync()
    {
        var goal = new Goal
        {
            UserId = 1,
            ExerciseId = goalExercise?.Id,
            Title = goalTitle,
            TargetValue = goalValue,
            CurrentValue = currentValue
        };

        await _goalService.AddGoalAsync(goal);
        
        GoalTitle = "";
        GoalExercise = null;
        GoalValue = 0;
        CurrentValue = 0;

        await LoadGoalsAsync();
    }
    
    public async Task LoadGoalsAsync()
    {
        var goalList = await _goalService.GetAllGoalsByUserAsync(1);
    
        Goals.Clear();

        foreach (var goal in goalList)
        {
            Goals.Add(goal);
        }
    }
    
    public async Task LoadExercisesAsync()
    {
        var exercisesList = await _exerciseService.GetAllExercisesAsync();
        
        Exercises.Clear();

        foreach (var exercise in exercisesList)
        {
            Exercises.Add(exercise);
        }
    }
}