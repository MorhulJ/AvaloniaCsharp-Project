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
    
    private async void OnDeleteSupplementIntake(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not SupplementIntakeViewModel vm) return;
        if (vm.SelectedSupplementIntake == null) return;

        var dialog = new ConfirmDialog();
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteSupplementIntakeCommand.ExecuteAsync(null);
        }
    }
}