using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;

namespace GymApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ExerciseViewModel _exerciseViewModel;
    private readonly GoalViewModel _goalViewModel;
    private readonly SupplementViewModel _supplementViewModel;
    private readonly NutritionViewModel _nutritionViewModel;
    private readonly PersonalRecordViewModel _personalRecordViewModel;
    private readonly SupplementIntakeViewModel _supplementIntakeViewModel;
    private readonly WorkoutProgramViewModel _workoutProgramViewModel;
    private readonly UserViewModel _userViewModel;
    
    [ObservableProperty]
    private ViewModelBase currentView;
    
    public MainWindowViewModel(ExerciseViewModel exerciseViewModel, GoalViewModel goalViewModel,  SupplementViewModel supplementViewModel, NutritionViewModel nutritionViewModel, PersonalRecordViewModel personalRecordViewModel, SupplementIntakeViewModel supplementIntakeViewModel, WorkoutProgramViewModel workoutProgramViewModel, UserViewModel userViewModel)
    {
        _exerciseViewModel = exerciseViewModel;
        _goalViewModel = goalViewModel;
        _supplementViewModel = supplementViewModel;
        _nutritionViewModel = nutritionViewModel;
        _personalRecordViewModel = personalRecordViewModel;
        _supplementIntakeViewModel = supplementIntakeViewModel;
        _workoutProgramViewModel = workoutProgramViewModel;
        _userViewModel = userViewModel;

        currentView = _workoutProgramViewModel;
    }
    
    [RelayCommand]
    private void ShowExercises()
    {
        CurrentView = _exerciseViewModel;
    }

    [RelayCommand]
    private void ShowGoals()
    {
        CurrentView = _goalViewModel;
    }
    
    [RelayCommand]
    private void ShowSupplements()
    {
        CurrentView = _supplementViewModel;
    }

    [RelayCommand]
    private void ShowNutrition()
    {
        CurrentView = _nutritionViewModel;
    }

    [RelayCommand]
    private void ShowPersonalRecord()
    {
        CurrentView = _personalRecordViewModel;
    }

    [RelayCommand]
    private void ShowSupplementIntake()
    {
        CurrentView = _supplementIntakeViewModel;
    }

    [RelayCommand]
    private void ShowWorkoutProgram()
    {
        CurrentView =  _workoutProgramViewModel;
    }
    
    [RelayCommand]
    private void ShowUser()
    {
        CurrentView =  _userViewModel;
    }
    
}