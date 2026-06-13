using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using System.Threading.Tasks;
namespace GymApp.ViewModels;

public partial class ExerciseViewModel : ViewModelBase
{
    private readonly  ExerciseService _exerciseService;
    
    public ObservableCollection<Exercise> Exercises { get; } = new();
    
    [ObservableProperty]
    private string exerciseName = "";
    [ObservableProperty]
    private string muscleGroup = "";
    [ObservableProperty]
    private string description = "";
    
    public ExerciseViewModel(ExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    [RelayCommand]
    private async Task SaveExerciseAsync()
    {
        var exercise = new Exercise
        {
            Name = exerciseName,
            MuscleGroup = muscleGroup,
            Description = description
        };
        
        await _exerciseService.AddExerciseAsync(exercise);

        ExerciseName = "";
        MuscleGroup = "";
        Description = "";

        await LoadExercisesAsync();
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