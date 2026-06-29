using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddGoalWindow : Window
{
    public AddGoalWindow(GoalViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void OnSaved()
        {
            Close();
            vm.GoalSaved -= OnSaved;
        }

        vm.GoalSaved += OnSaved;
    }
}