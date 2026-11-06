using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace GymApp.Mobile.Views;

public partial class RootView : UserControl
{
    private MobileDialog? _activeDialog;

    public RootView()
    {
        InitializeComponent();
        DetachedFromVisualTree += (_, _) => _activeDialog?.Close();
    }

    public async Task ShowDialogAsync(MobileDialog dialog)
    {
        if (_activeDialog != null) return;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnClosed(object? sender, EventArgs args) => completion.TrySetResult();
        _activeDialog = dialog;
        DialogHeading.Text = dialog.Title;
        dialog.Closed += OnClosed;
        DialogContent.Content = dialog;
        Workspace.IsEnabled = false;
        ModalLayer.IsVisible = true;
        try
        {
            Dispatcher.UIThread.Post(dialog.NotifyOpened, DispatcherPriority.Loaded);
            await completion.Task;
        }
        finally
        {
            dialog.Closed -= OnClosed;
            DialogContent.Content = null;
            ModalLayer.IsVisible = false;
            Workspace.IsEnabled = true;
            _activeDialog = null;
            Workspace.Focus();
        }
    }

    private void OnCloseDialog(object? sender, RoutedEventArgs args) => _activeDialog?.Close();
}
