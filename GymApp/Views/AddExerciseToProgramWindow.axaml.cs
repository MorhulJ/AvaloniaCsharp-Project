using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddExerciseToProgramWindow : Window
{
    public AddExerciseToProgramWindow(WorkoutProgramViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void OnSaved()
        {
            Close();
            vm.ProgramExerciseSaved -= OnSaved;
        }

        vm.ProgramExerciseSaved += OnSaved;
    }
}