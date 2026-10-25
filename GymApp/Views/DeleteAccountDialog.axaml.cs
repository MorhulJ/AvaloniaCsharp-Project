using Avalonia.Controls;

namespace GymApp.Views;

public partial class DeleteAccountDialog : Window
{
    public bool Confirmed { get; private set; }
    public string Password => PasswordInput.Text ?? "";

    public DeleteAccountDialog()
    {
        InitializeComponent();
    }

    private void OnDelete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorText.IsVisible = true;
            return;
        }

        Confirmed = true;
        Close();
    }

    private void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
