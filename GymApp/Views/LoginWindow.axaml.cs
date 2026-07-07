using Avalonia.Controls;
using GymApp.ViewModels;

namespace GymApp.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.LoginSuccessful += () => Close();
    }
}