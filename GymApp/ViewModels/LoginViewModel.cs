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
    private bool isRegistering = false;
    [ObservableProperty]
    private bool rememberMe;
    [ObservableProperty]
    private bool isRestoringSession;
    [ObservableProperty]
    private bool profileDataConsent;
    [ObservableProperty]
    private bool wasJustRegistered;
    
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
        WasJustRegistered = false;
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ValidationMessage = "Email and password are required";
            return;
        }

        ValidationMessage = "";
        var success = await _authService.LoginAsync(Login.Trim(), Password);

        if (!success)
        {
            ValidationMessage = _authService.LastErrorMessage ?? "Couldn't sign in. Please try again.";
            return;
        }

        UpdateSavedSignIn();
        Password = "";
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

        ValidationMessage = "";
        var success = await _authService.RegisterAsync(Login.Trim(), Password, Name.Trim(), ProfileDataConsent);

        if (!success)
        {
            ValidationMessage = _authService.LastErrorMessage ?? "Couldn't create the account. Please try again.";
            return;
        }

        WasJustRegistered = true;

        UpdateSavedSignIn();
        Password = "";
        LoginSuccessful?.Invoke();
    }

    private void UpdateSavedSignIn()
    {
        if (!_authService.SaveRememberMe(RememberMe))
            ValidationMessage = "Couldn't update saved sign-in on this device. You may need to sign in again next time.";
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
