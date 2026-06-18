using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class WorkoutProgramView : UserControl
{
    public WorkoutProgramView()
    {
        InitializeComponent();
        
        Loaded += async (_, _) =>
        {
            if (DataContext is WorkoutProgramViewModel vm)
            {
                await vm.LoadProgramsAsync();
                await vm.LoadExercisesAsync();
            }   
        };
    }
}