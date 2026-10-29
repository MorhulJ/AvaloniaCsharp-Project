using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;
using GymApp.Validators;

namespace GymApp.ViewModels;

public partial class UserViewModel : ViewModelBase
{
    private readonly UserService _userService;
    private readonly string _userId;
    private string _userEmail = "";
    
    [ObservableProperty]
    private string validationMessage = "";
    [ObservableProperty]
    private string userName = "";
    [ObservableProperty]
    private string userGender = "Male";
    [ObservableProperty]
    private DateTimeOffset? userDateOfBirth = new DateTimeOffset(new DateTime(1990, 1, 1));
    [ObservableProperty]
    private int userWeight;
    [ObservableProperty]
    private int userHeight;
    
    public List<string> GenderOptions { get; } = new() { "Male", "Female" };
    
    private readonly AuthService _authService;
    public event Action? LoggedOut;
    public event Action? AccountDeleted;
    
    public UserViewModel(UserService userService, string userId, AuthService authService)
    {
        _userService = userService;
        _userId = userId;
        _authService = authService;
        _userEmail = authService.CurrentEmail ?? "";
    }
    
    private readonly UserValidator _validator = new UserValidator();

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        var user = new User
        {
            FirebaseId = _userId,
            Name = UserName,
            Gender = UserGender,
            DateOfBirth = UserDateOfBirth?.DateTime ?? DateTime.MinValue,
            weight = UserWeight,
            height = UserHeight
        };
        
        var result = _validator.Validate(user);

        if (!result.IsValid)
        {
            ValidationMessage = result.Errors[0].ErrorMessage;
            return;
        }

        ValidationMessage = "";
        try { await _userService.UpdateUserAsync(user); }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "save your profile", canRetry: false);
            ValidationMessage = StatusMessage;
        }
    }

    public async Task LoadUserAsync()
    {
        SetStatusMessage("Loading your profile from Firebase…");
        try
        {
            var user = await _userService.GetUserByIdAsync(_userId);
            if (user == null)
            {
                ClearStatusMessage();
                return;
            }

            if (!string.IsNullOrWhiteSpace(user.Email)) _userEmail = user.Email;
            UserName = user.Name;
            UserGender = user.Gender;
            UserDateOfBirth = new DateTimeOffset(user.DateOfBirth);
            UserWeight = user.weight;
            UserHeight = user.height;
            ClearStatusMessage();
        }
        catch (Exception ex) { ReportCloudFailure(ex, "load your profile"); }
    }

    public override Task RetryAsync() => LoadUserAsync();
    
    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        LoggedOut?.Invoke();
    }

    public async Task DeleteAccountAsync(string password)
    {
        ValidationMessage = "";
        var stage = "checking the password";
        if (string.IsNullOrWhiteSpace(_userEmail))
        {
            ValidationMessage = "Could not find the account email. Sign out and sign in again, then retry.";
            return;
        }

        try
        {
            // Reauthentication gives Firebase a fresh sign-in timestamp, which it
            // requires for sensitive actions such as deleting an account.
            await _authService.ReauthenticateAsync(_userEmail, password);
            stage = "removing saved workout data";
            await _userService.DeleteUserAsync(_userId);
            stage = "deleting the Firebase account";
            await _authService.DeleteCurrentAccountAsync();
            AccountDeleted?.Invoke();
        }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, stage, canRetry: false);
            ValidationMessage = $"Account deletion stopped while {stage}. {GetCloudFailureMessage(ex, stage)}";
        }
    }
}
