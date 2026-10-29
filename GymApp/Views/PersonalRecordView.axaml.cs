using System;
using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class PersonalRecordView : UserControl
{
    public PersonalRecordView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            if (DataContext is PersonalRecordViewModel vm)
            {
                await vm.LoadExercisesAsync();
                await vm.LoadRecordsAsync(vm.CurrentUserId);
            }
        };
    }
    
    private void OpenAddRecordWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PersonalRecordViewModel vm) return;
        
        vm.ResetValidation();
        vm.RecordExercise = null;
        vm.RecordValue = 0;
        vm.RecordDate = DateTime.Today;

        var window = new AddRecordWindow(vm);
        window.ShowDialog(TopLevel.GetTopLevel(this) as Window);
    }
    
    private async void OnDeleteRecord(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PersonalRecordViewModel vm) return;
        if (vm.SelectedRecord == null) return;

        var record = vm.SelectedRecord;
        var exerciseName = record.Exercise?.Name ?? "this exercise";
        var dialog = new ConfirmDialog(
            "Delete personal record?",
            $"Delete the {record.Value} kg record for '{exerciseName}' dated {record.Date:dd.MM.yyyy}?");
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteRecordCommand.ExecuteAsync(null);
        }
    }
}
