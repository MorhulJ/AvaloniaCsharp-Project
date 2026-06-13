using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using System.Threading.Tasks;

namespace GymApp.ViewModels;

public partial class GoalsViewModel : ObservableObject
{
    private readonly GoalService _goalService;

    [ObservableProperty]
    private string goalTitle = "";
    [ObservableProperty]
    private Exercise? goalExercise;
    [ObservableProperty]
    private double goalValue;
    [ObservableProperty]
    private double currentValue;

    public GoalsViewModel(GoalService goalService)
    {
        _goalService = goalService;
    }

    [RelayCommand]
    private async Task SaveGoalAsync()
    {
        var goal = new Goal
        {
            UserId = 1,
            ExerciseId = GoalExercise?.Id,
            Title = GoalTitle,
            TargetValue = GoalValue,
            CurrentValue = CurrentValue
        };

        await _goalService.AddGoalAsync(goal);
        
        GoalTitle = "";
        GoalExercise = null;
        GoalValue = 0;
        CurrentValue = 0;
    }
}