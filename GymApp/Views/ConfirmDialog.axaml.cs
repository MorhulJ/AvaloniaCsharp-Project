using Avalonia.Controls;

namespace GymApp.Views;

public partial class ConfirmDialog : Window
{
    public bool Result { get; private set; } = false;

    public ConfirmDialog()
    {
        InitializeComponent();
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