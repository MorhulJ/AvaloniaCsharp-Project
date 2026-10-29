using Avalonia.Controls;
using Avalonia.Input;

namespace GymApp.Views;

public partial class ConfirmDialog : Window
{
    public bool Result { get; private set; } = false;

    public ConfirmDialog() : this("Confirm action", "Are you sure you want to continue?") { }

    public ConfirmDialog(string title, string message, string confirmText = "Delete")
    {
        InitializeComponent();
        DialogTitle.Text = title;
        DialogMessage.Text = message;
        ConfirmButton.Content = confirmText;
    }

    private void OnOpened(object? sender, System.EventArgs e) => CancelButton.Focus();

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        Result = false;
        Close();
        e.Handled = true;
    }

    private void OnYes(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void OnNo(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Result = false;
        Close();
    }
}
