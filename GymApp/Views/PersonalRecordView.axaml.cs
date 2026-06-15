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
}