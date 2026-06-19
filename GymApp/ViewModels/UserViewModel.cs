using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Models;
using GymApp.Services;

namespace GymApp.ViewModels;

public partial class UserViewModel : ViewModelBase
{
    private readonly UserService _userService;
    
    [ObservableProperty]
    private int userId;
    [ObservableProperty]
    private string userName = "";
    [ObservableProperty]
    private string userGender = "Male";
    [ObservableProperty]
    private DateTimeOffset? userDateOfBirth = new DateTimeOffset( new DateTime(1990, 1, 1));
    [ObservableProperty]
    private int userWeight;
    [ObservableProperty]
    private int userHeight;
    
    public List<string> GenderOptions { get; } = new() { "Male", "Female" };
    
    public UserViewModel(UserService userService)
    {
        _userService = userService;
    }

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        var user = new User
        {
            Id = UserId,
            Name = UserName,
            Gender = UserGender,
            DateOfBirth = UserDateOfBirth?.DateTime ?? DateTime.MinValue,
            weight =  UserWeight,
            height =  UserHeight
        };
        
        await _userService.UpdateUserAsync(user);
    }

    public async Task LoadUserAsync()
    {
        var user = await _userService.GetUserByIdAsync(1);
        
        if (user == null)
            return;
        
        UserId = user.Id;
        UserName = user.Name;
        UserGender = user.Gender;
        UserDateOfBirth = new DateTimeOffset(user.DateOfBirth);
        UserWeight = user.weight;
        UserHeight = user.height;
    }
}