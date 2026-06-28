using Avalonia.Controls;
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
                await vm.LoadGoalsAsync();
                await  vm.LoadExercisesAsync();
            }   
        };
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