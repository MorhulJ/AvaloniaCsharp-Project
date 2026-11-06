using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddSupplementWindow : GymApp.Mobile.Views.MobileDialog
{
    public AddSupplementWindow(SupplementViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void OnSaved()
        {
            Close();
            vm.SupplementSaved -= OnSaved;
        }

        vm.SupplementSaved += OnSaved;
        Closed += (_, _) => vm.SupplementSaved -= OnSaved;
    }
}