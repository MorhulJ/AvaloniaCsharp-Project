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
    private readonly  ExerciseService _exerciseService;
    
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
    
    private int? _editingExerciseId;
    
    public event Action? ExerciseSaved;
    
    public ExerciseViewModel(ExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }
    
    private readonly ExerciseValidator _validator = new();

    [RelayCommand]
    private async Task SaveExerciseAsync()
    {
        if (_editingExerciseId == null)
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

            await _exerciseService.AddExerciseAsync(exercise);
        }
        else
        {
            var exercise = new Exercise
            {
                Id = _editingExerciseId.Value,
                Name = exerciseName,
                MuscleGroup = muscleGroup,
                Description = description
            };

            await _exerciseService.UpdateExerciseAsync(exercise);
            _editingExerciseId = null;
        }

        ExerciseName = "";
        MuscleGroup = "";
        Description = "";

        await LoadExercisesAsync();

        ExerciseSaved?.Invoke();
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
    
    [RelayCommand]
    private async Task DeleteExerciseAsync()
    {
        if (SelectedExercise == null)
            return;

        await _exerciseService.DeleteExerciseAsync(SelectedExercise);
        
        ExerciseName = "";
        MuscleGroup = "";
        Description = "";
        _editingExerciseId = null;
        
        await LoadExercisesAsync();
    }
    
    partial void OnSelectedExerciseChanged(Exercise? value)
    {
        if (value == null) return;
    
        ExerciseName = value.Name;
        MuscleGroup = value.MuscleGroup;
        Description = value.Description;
        
        _editingExerciseId = value.Id;
    }
    
    public void ResetEditingState()
    {
        _editingExerciseId = null;
    }
    
    public void ResetValidation()
    {
        ValidationMessage = "";
    }
}