using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using System.Threading.Tasks;
using GymApp.Validators;

namespace GymApp.ViewModels;

public partial class ExerciseViewModel : ViewModelBase
{
    private readonly ExerciseService _exerciseService;
    
    public ObservableCollection<Exercise> Exercises { get; } = new();
    
    [ObservableProperty]
    private string validationMessage = "";
    [ObservableProperty]
    private string exerciseName = "";
    [ObservableProperty]
    private string muscleGroup = "";
    [ObservableProperty]
    private string description = "";
    [ObservableProperty]
    private Exercise? selectedExercise;
    
    private string? _editingExerciseFirebaseId;
    
    public string CurrentUserId { get; set; } = "";
    public event Action? ExerciseSaved;
    
    public ExerciseViewModel(ExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }
    
    private readonly ExerciseValidator _validator = new();

    [RelayCommand]
    private async Task SaveExerciseAsync()
    {
        if (_editingExerciseFirebaseId == null)
        {
            var exercise = new Exercise
            {
                Name = exerciseName,
                MuscleGroup = muscleGroup,
                Description = description
            };
            
            var result = _validator.Validate(exercise);

            if (!result.IsValid)
            {
                ValidationMessage = result.Errors[0].ErrorMessage;
                return;
            }

            ValidationMessage = "";
            await _exerciseService.AddExerciseAsync(exercise, CurrentUserId);
        }
        else
        {
            var exercise = new Exercise
            {
                FirebaseId = _editingExerciseFirebaseId,
                Name = exerciseName,
                MuscleGroup = muscleGroup,
                Description = description
            };

            await _exerciseService.UpdateExerciseAsync(exercise, CurrentUserId);
            _editingExerciseFirebaseId = null;
        }

        ExerciseName = "";
        MuscleGroup = "";
        Description = "";

        await LoadExercisesAsync(CurrentUserId);
        ExerciseSaved?.Invoke();
    }

    public async Task LoadExercisesAsync(string userId)
    {
        var exercisesList = await _exerciseService.GetAllExercisesAsync(userId);
        
        Exercises.Clear();
        foreach (var exercise in exercisesList)
            Exercises.Add(exercise);
    }
    
    [RelayCommand]
    private async Task DeleteExerciseAsync()
    {
        if (SelectedExercise == null)
            return;

        await _exerciseService.DeleteExerciseAsync(SelectedExercise, CurrentUserId);
        
        ExerciseName = "";
        MuscleGroup = "";
        Description = "";
        _editingExerciseFirebaseId = null;
        
        await LoadExercisesAsync(CurrentUserId);
    }
    
    partial void OnSelectedExerciseChanged(Exercise? value)
    {
        if (value == null) return;
    
        ExerciseName = value.Name;
        MuscleGroup = value.MuscleGroup;
        Description = value.Description;
        _editingExerciseFirebaseId = value.FirebaseId;
    }
    
    public void ResetEditingState()
    {
        _editingExerciseFirebaseId = null;
    }
    
    public void ResetValidation()
    {
        ValidationMessage = "";
    }
}