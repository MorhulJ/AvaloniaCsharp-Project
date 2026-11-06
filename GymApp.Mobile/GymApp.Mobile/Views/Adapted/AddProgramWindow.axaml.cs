using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddProgramWindow : GymApp.Mobile.Views.MobileDialog
{
    public AddProgramWindow(WorkoutProgramViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void OnSaved()
        {
            Close();
            vm.ProgramSaved -= OnSaved;
        }

        vm.ProgramSaved += OnSaved;
        Closed += (_, _) => vm.ProgramSaved -= OnSaved;
    }
}