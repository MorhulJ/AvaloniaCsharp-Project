using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GymApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ExerciseViewModel _exerciseViewModel;
    private readonly GoalViewModel _goalViewModel;
    private readonly SupplementViewModel _supplementViewModel;
    private readonly NutritionViewModel _nutritionViewModel;
    private readonly PersonalRecordViewModel _personalRecordViewModel;
    
    [ObservableProperty]
    private ViewModelBase currentView;
    
    public MainWindowViewModel(ExerciseViewModel exerciseViewModel, GoalViewModel goalViewModel,  SupplementViewModel supplementViewModel, NutritionViewModel nutritionViewModel, PersonalRecordViewModel personalRecordViewModel)
    {
        _exerciseViewModel = exerciseViewModel;
        _goalViewModel = goalViewModel;
        _supplementViewModel = supplementViewModel;
        _nutritionViewModel = nutritionViewModel;
        _personalRecordViewModel = personalRecordViewModel;

        currentView = _exerciseViewModel;
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
        CurrentView =  _nutritionViewModel;
    }

    [RelayCommand]
    private void ShowPersonalRecord()
    {
        CurrentView =  _personalRecordViewModel;
    }
}