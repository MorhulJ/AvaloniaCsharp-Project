using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class UserView : UserControl
{
    public UserView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            if (DataContext is UserViewModel vm)
            {
                await vm.LoadUserAsync();
                vm.ValidationMessage = "";
            }
        };
    }
    
    private async void OnDeleteAccount(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not UserViewModel vm) return;

        var dialog = new ConfirmDialog();
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteAccountCommand.ExecuteAsync(null);
        }
    }
}