using System;
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
                await vm.LoadSupplementsAsync(vm.CurrentUserId);
                await vm.LoadSupplementIntakesAsync(vm.CurrentUserId);
            }
        };
    }
    
    private void OpenAddIntakeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementIntakeViewModel vm) return;
        
        vm.ResetValidation();
        vm.Supplement = null;
        vm.SupplementDosage = 0;
        vm.SupplementDay = DayOfWeek.Monday;
        vm.SupplementTime = TimeSpan.Zero;

        var window = new AddIntakeWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }
    
    private async void OnDeleteSupplementIntake(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementIntakeViewModel vm) return;
        if (vm.SelectedSupplementIntake == null) return;

        var intake = vm.SelectedSupplementIntake;
        var supplementName = intake.Supplement?.Name ?? "this supplement";
        var dialog = new ConfirmDialog(
            "Delete schedule entry?",
            $"Delete the {supplementName} reminder for {intake.Date} at {intake.Time}?");
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteSupplementIntakeCommand.ExecuteAsync(null);
        }
    }
}
