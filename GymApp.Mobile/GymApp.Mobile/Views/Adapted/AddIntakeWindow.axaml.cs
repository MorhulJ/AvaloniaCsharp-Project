using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddIntakeWindow : GymApp.Mobile.Views.MobileDialog
{
    public AddIntakeWindow(SupplementIntakeViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void OnSaved()
        {
            Close();
            vm.IntakeSaved -= OnSaved;
        }

        vm.IntakeSaved += OnSaved;
        Closed += (_, _) => vm.IntakeSaved -= OnSaved;
    }
}