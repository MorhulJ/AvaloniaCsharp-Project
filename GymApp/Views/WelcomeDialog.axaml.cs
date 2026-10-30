using Avalonia.Controls;

namespace GymApp.Views;

public partial class WelcomeDialog : Window
{
    public WelcomeDialog()
    {
        InitializeComponent();
    }

    private void OnOpened(object? sender, System.EventArgs e) => ContinueButton.Focus();

    private void OnContinue(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
