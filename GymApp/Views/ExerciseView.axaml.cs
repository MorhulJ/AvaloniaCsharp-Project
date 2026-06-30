using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class ExerciseView : UserControl
{
    public ExerciseView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            if (DataContext is ExerciseViewModel vm)
            {
                await vm.LoadExercisesAsync();
            }
        };
    }

    private void OpenAddExerciseWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ExerciseViewModel vm) return;
        
        vm.ResetEditingState();
        vm.ExerciseName = "";
        vm.MuscleGroup = "";
        vm.Description = "";

        var window = new AddExerciseWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }

    private void OpenEditExerciseWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ExerciseViewModel vm) return;
        if (vm.SelectedExercise == null) return;

        var window = new AddExerciseWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }
    
    private async void OnDeleteExercise(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not ExerciseViewModel vm) return;
        if (vm.SelectedExercise == null) return;

        var dialog = new ConfirmDialog();
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteExerciseCommand.ExecuteAsync(null);
        }
    }
}