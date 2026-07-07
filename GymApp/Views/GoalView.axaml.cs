using Avalonia.Controls;
using GymApp.Models;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class GoalView : UserControl
{
    public GoalView()
    {
        InitializeComponent();
        
        Loaded += async (_, _) =>
        {
            if (DataContext is GoalViewModel vm)
            {
                await vm.LoadGoalsAsync(vm.CurrentUserId);
                await  vm.LoadExercisesAsync();
            }   
        };
    }
    
    private void OpenAddGoalWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not GoalViewModel vm) return;
        
        vm.ResetEditingState();
        vm.ResetValidation();
        vm.GoalTitle = "";
        vm.GoalExercise = null;
        vm.GoalValue = 0;
        vm.CurrentValue = 0;

        var window = new AddGoalWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }

    private void OpenEditGoalWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not GoalViewModel vm) return;
        if (vm.SelectedGoal == null) return;

        var window = new AddGoalWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }
    
    private async void OnDeleteGoal(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not GoalViewModel vm) return;
        if (vm.SelectedGoal == null) return;

        var dialog = new ConfirmDialog();
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteGoalCommand.ExecuteAsync(null);
        }
    }
}