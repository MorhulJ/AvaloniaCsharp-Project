using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddExerciseWindow : Window
{
    public AddExerciseWindow(ExerciseViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.ExerciseSaved += () =>
        {
            Close();
        };
    }
}