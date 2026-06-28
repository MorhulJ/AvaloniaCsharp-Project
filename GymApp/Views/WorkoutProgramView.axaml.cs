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
                await vm.LoadProgramsAsync();
                await vm.LoadExercisesAsync();
            }   
        };
    }
    
    private async void OnDeleteWorkoutProgram(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkoutProgramViewModel vm) return;
        if (vm.SelectedProgram == null) return;

        var dialog = new ConfirmDialog();
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

        var dialog = new ConfirmDialog();
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteExerciseFromProgramCommand.ExecuteAsync(null);
        }
    }
}