using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddIntakeWindow : Window
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
    }
}