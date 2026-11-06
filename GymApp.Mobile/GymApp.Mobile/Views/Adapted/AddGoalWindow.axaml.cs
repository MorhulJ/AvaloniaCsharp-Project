using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddGoalWindow : GymApp.Mobile.Views.MobileDialog
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
        Closed += (_, _) => vm.GoalSaved -= OnSaved;
    }
}