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

    private void OnOpened(object? sender, System.EventArgs e)
    {
        if (DataContext is LoginViewModel { IsRegistering: true })
            NameInput.Focus();
        else
            EmailInput.Focus();
    }
}
