using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Services;
using System;
using System.Threading.Tasks;

namespace GymApp.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    [ObservableProperty] 
    private string login = "";
    [ObservableProperty] 
    private string password = "";
    [ObservableProperty] 
    private string name = "";
    [ObservableProperty] 
    private string validationMessage = "";
    [ObservableProperty] 
    private bool rememberMe = false;
    [ObservableProperty] 
    private bool isRegistering = false;
    
    public string ModeTitle => IsRegistering ? "Create account" : "Sign in";
    public string ToggleModeText => IsRegistering ? "Already have an account? Login" : "Don't have an account? Register";

    public event Action? LoginSuccessful;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ValidationMessage = "Login and password are required";
            return;
        }

        var user = await _authService.LoginAsync(Login, Password);

        if (user == null)
        {
            ValidationMessage = "Invalid login or password";
            return;
        }

        if (RememberMe)
            _authService.SaveRememberMe(user.Id);

        LoginSuccessful?.Invoke();
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "All fields are required";
            return;
        }

        var success = await _authService.RegisterAsync(Login, Password, Name);

        if (!success)
        {
            ValidationMessage = "Login already exists";
            return;
        }

        if (RememberMe)
            _authService.SaveRememberMe(_authService.CurrentUser!.Id);

        LoginSuccessful?.Invoke();
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegistering = !IsRegistering;
        ValidationMessage = "";
    }
    
    partial void OnIsRegisteringChanged(bool value)
    {
        OnPropertyChanged(nameof(ModeTitle));
        OnPropertyChanged(nameof(ToggleModeText));
    }
}