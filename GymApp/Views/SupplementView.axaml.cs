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
    
    private void OpenAddSupplementWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementViewModel vm) return;

        vm.SupplementName = "";
        vm.DosageUnit = "";
        vm.Description = "";

        var window = new AddSupplementWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }

    private void OpenEditSupplementWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementViewModel vm) return;
        if (vm.SelectedSupplement == null) return;

        var window = new AddSupplementWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }
}