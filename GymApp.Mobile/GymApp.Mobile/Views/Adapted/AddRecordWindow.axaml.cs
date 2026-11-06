using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddRecordWindow : GymApp.Mobile.Views.MobileDialog
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
        Closed += (_, _) => vm.RecordSaved -= OnSaved;
    }
}