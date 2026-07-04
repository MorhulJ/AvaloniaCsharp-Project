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
        
        vm.ResetEditingState();
        vm.ResetValidation();
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
    
    private async void OnDeleteSupplement(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementViewModel vm) return;
        if (vm.SelectedSupplement == null) return;

        var dialog = new ConfirmDialog();
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteSupplementCommand.ExecuteAsync(null);
        }
    }
}