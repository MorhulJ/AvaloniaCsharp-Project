using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using GymApp.ViewModels;

namespace GymApp.Mobile.Views;

public partial class FeaturePage : UserControl
{
    public FeaturePage() => InitializeComponent();

    public FeaturePage(Control page, ViewModelBase viewModel) : this()
    {
        DataContext = viewModel;
        PageContent.Content = page;
    }

    private async void OnRetry(object? sender, RoutedEventArgs args)
    {
        if (DataContext is not ViewModelBase viewModel || sender is not Button button) return;
        button.IsEnabled = false;
        try
        {
            await viewModel.RetryAsync();
        }
        catch (Exception exception)
        {
            Trace.TraceError($"Mobile page retry failed: {exception}");
            StatusText.SetCurrentValue(TextBlock.TextProperty,
                "Couldn't reload your data. Check your connection and try again.");
        }
        finally
        {
            button.IsEnabled = true;
        }
    }
}
