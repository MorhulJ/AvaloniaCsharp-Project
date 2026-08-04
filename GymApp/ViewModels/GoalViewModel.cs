using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using System.Threading.Tasks;
using GymApp.Validators;

namespace GymApp.ViewModels;

public partial class GoalViewModel : ViewModelBase
{
    private readonly GoalService _goalService;
    private readonly ExerciseService _exerciseService;

    public ObservableCollection<Goal> Goals { get; } = new();

    public ObservableCollection<Exercise> Exercises { get; } = new();

    [ObservableProperty] 
    private string validationMessage = "";

    [ObservableProperty] 
    private string goalTitle = "";
    [ObservableProperty] 
    private Exercise? goalExercise;
    [ObservableProperty] 
    private double goalValue;
    [ObservableProperty] 
    private double currentValue;
    [ObservableProperty] 
    private Goal? selectedGoal;

    private string? _editingGoalFirebaseId;
    
    public string CurrentUserId { get; set; }
    
    public event Action? GoalSaved;

    public GoalViewModel(GoalService goalService, ExerciseService exerciseService)
    {
        _goalService = goalService;
        _exerciseService = exerciseService;
    }

    private readonly GoalValidator _validator = new();

    [RelayCommand]
    private async Task SaveGoalAsync()
    {
        if (_editingGoalFirebaseId == null)
        {
            var goal = new Goal
            {
                UserId = CurrentUserId,
                ExerciseFirebaseId = goalExercise?.FirebaseId,
                Title = goalTitle,
                TargetValue = goalValue,
                CurrentValue = currentValue
            };
            
            var result = _validator.Validate(goal);

            if (!result.IsValid)
            {
                ValidationMessage = result.Errors[0].ErrorMessage;
                return;
            }

            ValidationMessage = "";

            await _goalService.AddGoalAsync(goal);
        }
        else
        {
            var goal = new Goal
            {
                FirebaseId = _editingGoalFirebaseId,
                Title = goalTitle,
                ExerciseFirebaseId = goalExercise?.FirebaseId,
                TargetValue = goalValue,
                CurrentValue = currentValue
            };

            await _goalService.UpdateGoalAsync(goal);

            _editingGoalFirebaseId = null;
        }

        GoalTitle = "";
        GoalExercise = null;
        GoalValue = 0;
        CurrentValue = 0;

        await LoadGoalsAsync(CurrentUserId);
        
        GoalSaved?.Invoke();
    }

    public async Task LoadGoalsAsync(string userId)
    {
        var goalList = await _goalService.GetAllGoalsByUserAsync(userId);

        Goals.Clear();

        foreach (var goal in goalList)
        {
            Goals.Add(goal);
        }
    }

    public async Task LoadExercisesAsync()
    {
        var exercisesList = await _exerciseService.GetAllExercisesAsync(CurrentUserId);

        Exercises.Clear();

        foreach (var exercise in exercisesList)
        {
            Exercises.Add(exercise);
        }
    }

    [RelayCommand]
    private async Task DeleteGoalAsync()
    {
        if (SelectedGoal == null)
            return;

        await _goalService.DeleteGoalAsync(SelectedGoal);
        await LoadGoalsAsync(CurrentUserId);
    }

    partial void OnSelectedGoalChanged(Goal? value)
    {
        if (value == null)
            return;

        GoalTitle = value.Title;
        GoalExercise = Exercises.FirstOrDefault(e => e.FirebaseId == value.ExerciseFirebaseId);
        GoalValue = value.TargetValue;
        CurrentValue = value.CurrentValue;

        _editingGoalFirebaseId = value.FirebaseId;
    }
    
    public void ResetEditingState()
    {
        _editingGoalFirebaseId = null;
    }

    public void ResetValidation()
    {
        validationMessage = "";
    }
}