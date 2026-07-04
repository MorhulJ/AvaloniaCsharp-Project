using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class NutritionView : UserControl
{
    public NutritionView()
    {
        InitializeComponent();
        
        Loaded += (_, _) =>
        {
            if (DataContext is NutritionViewModel vm)
            {
                vm.ValidationMessage = "";
            }
        };
    }
}