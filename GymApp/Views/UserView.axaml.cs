using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class UserView : UserControl
{
    public UserView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            if (DataContext is UserViewModel vm)
            {
                await vm.LoadUserAsync();
                vm.ValidationMessage = "";
            }
        };
    }
}