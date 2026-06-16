using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class SupplementIntakeView : UserControl
{
    public SupplementIntakeView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            if (DataContext is SupplementIntakeViewModel vm)
            {
                await vm.LoadSupplementIntakesAsync();
                await vm.LoadSupplementsAsync();
            }
        };
    }
}