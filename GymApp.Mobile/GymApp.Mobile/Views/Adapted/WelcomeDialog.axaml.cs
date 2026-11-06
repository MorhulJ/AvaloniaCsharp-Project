using Avalonia.Controls;

namespace GymApp.Views;

public partial class WelcomeDialog : GymApp.Mobile.Views.MobileDialog
{
    public WelcomeDialog()
    {
        InitializeComponent();
    }

    private void OnOpened(object? sender, System.EventArgs e) => ContinueButton.Focus();

    private void OnContinue(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
