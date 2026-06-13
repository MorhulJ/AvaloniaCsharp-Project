using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
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
}