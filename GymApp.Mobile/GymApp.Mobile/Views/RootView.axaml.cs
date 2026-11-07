using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace GymApp.Mobile.Views;

public partial class RootView : UserControl
{
    private MobileDialog? _activeDialog;
    private TopLevel? _topLevel;
    private IInputPane? _inputPane;

    public RootView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) =>
        {
            if (!OperatingSystem.IsIOS()) return;
            _topLevel = TopLevel.GetTopLevel(this);
            _inputPane = _topLevel?.InputPane;
            if (_inputPane is not null)
            {
                _inputPane.StateChanged += OnInputPaneChanged;
                UpdateKeyboardInset(_inputPane.OccludedRect);
            }
        };
        DetachedFromVisualTree += (_, _) =>
        {
            if (_inputPane is not null) _inputPane.StateChanged -= OnInputPaneChanged;
            _inputPane = null;
            _topLevel = null;
            KeyboardLayout.Margin = default;
            _activeDialog?.Close();
        };
    }

    private void OnInputPaneChanged(object? sender, InputPaneStateEventArgs args) =>
        UpdateKeyboardInset(args.NewState == InputPaneState.Closed ? default : args.EndRect);

    private void UpdateKeyboardInset(Rect keyboard)
    {
        var bottom = _topLevel is null ? null
            : this.TranslatePoint(new Point(0, Bounds.Height - Padding.Bottom), _topLevel);
        var inset = bottom.HasValue && keyboard.Height > 0
            ? Math.Max(0, bottom.Value.Y - keyboard.Top) : 0;
        KeyboardLayout.Margin = new Thickness(0, 0, 0, inset);
        if (inset > 0 && _topLevel?.FocusManager?.GetFocusedElement() is Control focused)
            Dispatcher.UIThread.Post(() => focused.BringIntoView(), DispatcherPriority.Loaded);
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
