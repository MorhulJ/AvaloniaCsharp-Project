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
}