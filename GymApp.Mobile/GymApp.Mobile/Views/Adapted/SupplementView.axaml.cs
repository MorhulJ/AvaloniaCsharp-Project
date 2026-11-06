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
                await vm.LoadSupplementAsync(vm.CurrentUserId);
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
        window.ShowDialog(this);
    }

    private void OpenEditSupplementWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementViewModel vm) return;
        if (vm.SelectedSupplement == null) return;

        var selected = vm.SelectedSupplement;
        vm.SelectedSupplement = null;
        vm.SelectedSupplement = selected;

        var window = new AddSupplementWindow(vm);
        window.ShowDialog(this);
    }

    private async void OnDeleteSupplement(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementViewModel vm) return;
        if (vm.SelectedSupplement == null) return;

        var dialog = new ConfirmDialog(
            "Delete supplement?",
            $"This will permanently delete '{vm.SelectedSupplement.Name}' from your supplements.");
        await dialog.ShowDialog(this);

        if (dialog.Result)
        {
            await vm.DeleteSupplementCommand.ExecuteAsync(null);
        }
    }
}
