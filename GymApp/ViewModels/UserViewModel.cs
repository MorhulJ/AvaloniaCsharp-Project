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
        await _userService.UpdateUserAsync(user);
    }

    public async Task LoadUserAsync()
    {
        var user = await _userService.GetUserByIdAsync(_userId);
        
        if (user == null)
            return;
        
        UserName = user.Name;
        UserGender = user.Gender;
        UserDateOfBirth = new DateTimeOffset(user.DateOfBirth);
        UserWeight = user.weight;
        UserHeight = user.height;
    }
    
    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        LoggedOut?.Invoke();
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        await _userService.DeleteUserAsync(_userId);
        _authService.Logout();
        AccountDeleted?.Invoke();
    }
}