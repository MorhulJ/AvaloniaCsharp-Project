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
    private bool _profileLoaded;
    
    [ObservableProperty]
    private string validationMessage = "";
    [ObservableProperty]
    private string userName = "";
    [ObservableProperty]
    private string userGender = "";
    [ObservableProperty]
    private DateTimeOffset? userDateOfBirth;
    [ObservableProperty]
    private int userWeight;
    [ObservableProperty]
    private int userHeight;
    [ObservableProperty]
    private bool profileDataConsentGranted;
    [ObservableProperty]
    private bool profileDataConsentChoice;
    
    public List<string> GenderOptions { get; } = new() { "Male", "Female" };
    
    private readonly AuthService _authService;
    public event Action? LoggedOut;
    public event Action? AccountDeleted;
    public event Action<bool>? ProfileDataConsentChanged;
    
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
            ProfileDataConsent = ProfileDataConsentGranted
        };

        if (ProfileDataConsentGranted)
        {
            user.Gender = UserGender;
            user.DateOfBirth = UserDateOfBirth?.DateTime ?? DateTime.MinValue;
            user.weight = UserWeight;
            user.height = UserHeight;
        }

        var result = _validator.Validate(user);

        if (!result.IsValid)
        {
            ValidationMessage = result.Errors[0].ErrorMessage;
            return;
        }

        try
        {
            await _userService.UpdateUserAsync(user);
            ValidationMessage = "Profile saved.";
        }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "save your profile", canRetry: false);
            ValidationMessage = GetCloudFailureMessage(ex, "save your profile");
        }
    }

    public async Task LoadUserAsync()
    {
        if (_profileLoaded) return;

        SetStatusMessage("Loading your profile from Firebase…");
        try
        {
            var user = await _userService.GetUserByIdAsync(_userId);
            if (user == null)
            {
                ClearStatusMessage();
                _profileLoaded = true;
                return;
            }

            if (!string.IsNullOrWhiteSpace(user.Email)) _userEmail = user.Email;
            UserName = user.Name;
            ProfileDataConsentGranted = user.ProfileDataConsent;
            if (ProfileDataConsentGranted)
            {
                UserGender = user.Gender;
                UserDateOfBirth = user.DateOfBirth == DateTime.MinValue
                    ? null
                    : new DateTimeOffset(user.DateOfBirth);
                UserWeight = user.weight;
                UserHeight = user.height;
            }
            else
            {
                UserGender = "";
                UserDateOfBirth = null;
                UserWeight = 0;
                UserHeight = 0;
            }
            ProfileDataConsentChanged?.Invoke(ProfileDataConsentGranted);
            ClearStatusMessage();
            _profileLoaded = true;
        }
        catch (Exception ex) { ReportCloudFailure(ex, "load your profile"); }
    }

    public override Task RetryAsync()
    {
        _profileLoaded = false;
        return LoadUserAsync();
    }

    [RelayCommand]
    private async Task GrantProfileDataConsentAsync()
    {
        ValidationMessage = "";
        if (!ProfileDataConsentChoice)
        {
            ValidationMessage = "Check the consent box to enable these optional profile features.";
            return;
        }

        try
        {
            await _userService.GrantProfileDataConsentAsync(_userId);
            ProfileDataConsentGranted = true;
            ProfileDataConsentChoice = false;
            _profileLoaded = false;
            await LoadUserAsync();
            ProfileDataConsentChanged?.Invoke(true);
        }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "save your consent", canRetry: false);
            ValidationMessage = GetCloudFailureMessage(ex, "save your consent");
        }
    }

    public async Task WithdrawProfileDataConsentAsync()
    {
        ValidationMessage = "";
        ProfileDataConsentGranted = false;
        ProfileDataConsentChoice = false;
        UserGender = "";
        UserDateOfBirth = null;
        UserWeight = 0;
        UserHeight = 0;
        ProfileDataConsentChanged?.Invoke(false);

        try
        {
            await _userService.RevokeProfileDataConsentAsync(_userId);
            ValidationMessage = "Consent withdrawn. Saved profile measurements were deleted.";
        }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "withdraw profile data consent", canRetry: false);
            ValidationMessage = GetCloudFailureMessage(ex, "withdraw profile data consent");
        }
    }

    public async Task DeleteUnconsentedProfileDataAsync()
    {
        ValidationMessage = "";
        try
        {
            await _userService.DeleteUnconsentedProfileDataAsync(_userId);
            UserGender = "";
            UserDateOfBirth = null;
            UserWeight = 0;
            UserHeight = 0;
            ValidationMessage = "Saved profile measurements were deleted.";
        }
        catch (Exception ex)
        {
            ReportCloudFailure(ex, "delete saved profile measurements", canRetry: false);
            ValidationMessage = GetCloudFailureMessage(ex, "delete saved profile measurements");
        }
    }
    
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
