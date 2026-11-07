using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace GymApp.Mobile.iOS;

internal static class ReportShareSheet
{
    public static async Task<bool> ShowAsync(string path)
    {
        var presenter = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .Where(scene => scene.ActivationState == UISceneActivationState.ForegroundActive)
            .SelectMany(scene => scene.Windows)
            .FirstOrDefault(window => window.IsKeyWindow)?.RootViewController
            ?? throw new InvalidOperationException("No active window to share the report.");

        while (presenter.PresentedViewController is { } presented)
            presenter = presented;

        using var url = NSUrl.FromFilename(path);
        using var controller = new UIActivityViewController(new NSObject[] { url }, null);
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        controller.CompletionWithItemsHandler = (_, completed, _, error) =>
        {
            if (error is not null)
                completion.TrySetException(new InvalidOperationException("iOS couldn't share the report."));
            else
                completion.TrySetResult(completed);
        };

        presenter.PresentViewController(controller, true, null);
        return await completion.Task;
    }
}
