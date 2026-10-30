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

        var dialog = new DeleteAccountDialog();
        if (TopLevel.GetTopLevel(this) is not Window owner) return;
        await dialog.ShowDialog(owner);

        if (dialog.Confirmed)
            await vm.DeleteAccountAsync(dialog.Password);
    }

    private async void OnWithdrawProfileDataConsent(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not UserViewModel vm) return;
        var dialog = new ConfirmDialog(
            "Withdraw profile data consent?",
            "This deletes your saved gender, date of birth, weight and height from your GymApp profile and disables the Nutrition Calculator.",
            "Withdraw consent");
        if (TopLevel.GetTopLevel(this) is not Window owner) return;
        await dialog.ShowDialog(owner);
        if (dialog.Result) await vm.WithdrawProfileDataConsentAsync();
    }

    private async void OnDeleteUnconsentedData(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not UserViewModel vm) return;
        var dialog = new ConfirmDialog(
            "Delete saved measurements?",
            "This removes any older profile measurements saved before GymApp started asking for consent. Your account and workout data will remain.",
            "Delete measurements");
        if (TopLevel.GetTopLevel(this) is not Window owner) return;
        await dialog.ShowDialog(owner);
        if (dialog.Result) await vm.DeleteUnconsentedProfileDataAsync();
    }
}
