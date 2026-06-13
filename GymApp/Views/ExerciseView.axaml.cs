using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class ExerciseView : UserControl
{
    public ExerciseView()
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