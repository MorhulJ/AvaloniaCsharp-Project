using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class SupplementView : UserControl
{
    public SupplementView()
    {
        InitializeComponent();
        
        Loaded += async (_, _) =>
        {
            if (DataContext is SupplementViewModel vm)
            {
                await vm.LoadSupplementAsync();
            }   
        };
    }
}