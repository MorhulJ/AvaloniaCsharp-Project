using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace GymApp.Mobile.Views;

public class MobileDialog : UserControl
{
    public string Title { get; set; } = "";

    public event EventHandler? Opened;
    public event EventHandler? Closed;

    public Task ShowDialog(Control source)
    {
        var root = source.GetVisualAncestors().OfType<RootView>().FirstOrDefault();
        return root?.ShowDialogAsync(this) ?? Task.CompletedTask;
    }

    internal void NotifyOpened() => Opened?.Invoke(this, EventArgs.Empty);

    public void Close() => Closed?.Invoke(this, EventArgs.Empty);
}
