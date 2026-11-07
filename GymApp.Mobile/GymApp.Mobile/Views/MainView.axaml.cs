
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GymApp.Mobile.ViewModels;
using System;
using System.Diagnostics;
using System.IO;

namespace GymApp.Mobile.Views;

public partial class MainView : TabbedPage
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void OnExportReport(object? sender, RoutedEventArgs args)
    {
        if (DataContext is not MainViewModel viewModel || sender is not Button button) return;
        button.IsEnabled = false;
        string? exportDirectory = null;
        string? exportPath = null;
        ShowReportMessage("Preparing PDF report…");
        try
        {
            var name = $"GymApp-report-{DateTime.Now:yyyy-MM-dd}.pdf";
            exportDirectory = Path.Combine(Path.GetTempPath(), $"gymapp-export-{Guid.NewGuid():N}");
            Directory.CreateDirectory(exportDirectory);
            exportPath = Path.Combine(exportDirectory, name);
            await viewModel.CreateReportAsync(exportPath);

            bool exported;
            if (Application.Current is App { ShareReportAsync: { } share })
            {
                exported = await share(exportPath);
            }
            else
            {
                var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
                if (storage is not { CanSave: true }) throw new IOException("Saving is unavailable.");
                var destination = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save workout report",
                    SuggestedFileName = name,
                    DefaultExtension = "pdf",
                    FileTypeChoices = [new FilePickerFileType("PDF document") { Patterns = ["*.pdf"] }]
                });
                exported = destination is not null;
                if (destination is not null)
                {
                    using (destination)
                    await using (var output = await destination.OpenWriteAsync())
                    await using (var source = File.OpenRead(exportPath))
                    {
                        if (output.CanSeek) output.SetLength(0);
                        await source.CopyToAsync(output);
                    }
                }
            }

            if (exported) ShowReportMessage("PDF report exported.", success: true);
            else ReportMessage.IsVisible = false;
        }
        catch (Exception error)
        {
            Trace.TraceWarning($"PDF export failed: {error.GetType().Name}");
            ShowReportMessage("Couldn't export the report. Check your connection and try again.", failure: true);
        }
        finally
        {
            if (exportDirectory is not null)
            {
                try
                {
                    if (exportPath is not null) File.Delete(exportPath);
                    Directory.Delete(exportDirectory);
                }
                catch (IOException) { Trace.TraceWarning("Couldn't remove the temporary report."); }
                catch (UnauthorizedAccessException) { Trace.TraceWarning("Couldn't remove the temporary report."); }
            }
            button.IsEnabled = true;
        }
    }

    private void ShowReportMessage(string message, bool success = false, bool failure = false)
    {
        ReportMessage.Text = message;
        ReportMessage.Classes.Set("feedback", success || failure);
        ReportMessage.Classes.Set("success", success);
        ReportMessage.IsVisible = true;
    }
}
