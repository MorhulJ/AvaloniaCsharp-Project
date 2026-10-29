using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class WorkoutProgramView : UserControl
{
    public WorkoutProgramView()
    {
        InitializeComponent();
        
        Loaded += async (_, _) =>
        {
            if (DataContext is WorkoutProgramViewModel vm)
            {
                await vm.LoadExercisesAsync();
                await vm.LoadProgramsAsync(vm.CurrentUserId);
            }   
        };
    }
    
    private void OpenAddProgramWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkoutProgramViewModel vm) return;

        vm.ResetEditingState();
        vm.ResetProgramValidation();
        vm.ProgramName = "";
        vm.ProgramDescription = "";

        var window = new AddProgramWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }

    private void OpenEditProgramWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkoutProgramViewModel vm) return;
        if (vm.SelectedProgram == null) return;

        var window = new AddProgramWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }
    
    private void OpenAddExerciseToProgramWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkoutProgramViewModel vm) return;
        if (vm.SelectedProgram == null) return;

        vm.SelectedExercise = null;
        vm.ResetProgramExerciseValidation();
        vm.ExerciseSets = 0;
        vm.ExerciseReps = 0;
        vm.ExerciseRestTime = 0;

        var window = new AddExerciseToProgramWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }
    
    private async void OnDeleteWorkoutProgram(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkoutProgramViewModel vm) return;
        if (vm.SelectedProgram == null) return;

        var dialog = new ConfirmDialog(
            "Delete workout program?",
            $"This will permanently delete '{vm.SelectedProgram.Name}' and its exercises.");
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteProgramCommand.ExecuteAsync(null);
        }
    }
    
    private async void OnDeleteExerciseFromProgram(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkoutProgramViewModel vm) return;
        if (vm.SelectedProgramExercise == null) return;

        var exerciseName = vm.SelectedProgramExercise.Exercise?.Name ?? "this exercise";
        var dialog = new ConfirmDialog(
            "Remove exercise from program?",
            $"Remove '{exerciseName}' from '{vm.SelectedProgram?.Name}'?");
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteExerciseFromProgramCommand.ExecuteAsync(null);
        }
    }
}
