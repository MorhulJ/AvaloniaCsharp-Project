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
                await vm.LoadRecordsAsync();
                await vm.LoadExercisesAsync();
            }
        };
    }
    
    private void OpenAddRecordWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PersonalRecordViewModel vm) return;

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

        var dialog = new ConfirmDialog();
        await dialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);

        if (dialog.Result)
        {
            await vm.DeleteRecordCommand.ExecuteAsync(null);
        }
    }
}