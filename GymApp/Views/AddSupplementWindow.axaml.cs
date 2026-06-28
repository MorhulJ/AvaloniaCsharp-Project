using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class AddSupplementWindow : Window
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
    }
}