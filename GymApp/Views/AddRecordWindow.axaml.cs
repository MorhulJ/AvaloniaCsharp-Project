using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddRecordWindow : Window
{
    public AddRecordWindow(PersonalRecordViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void OnSaved()
        {
            Close();
            vm.RecordSaved -= OnSaved;
        }

        vm.RecordSaved += OnSaved;
    }
}