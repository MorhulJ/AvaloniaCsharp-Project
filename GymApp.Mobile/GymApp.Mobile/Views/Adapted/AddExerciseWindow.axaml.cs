using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddExerciseWindow : GymApp.Mobile.Views.MobileDialog
{
    public AddExerciseWindow(ExerciseViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void OnSaved()
        {
            Close();
            vm.ExerciseSaved -= OnSaved;
        }

        vm.ExerciseSaved += OnSaved;
        Closed += (_, _) => vm.ExerciseSaved -= OnSaved;
    }
}