using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GymApp.Services;
using GymApp.ViewModels;

namespace GymApp.Mobile.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly AuthService _authService = new();
    private readonly LoginViewModel _loginViewModel;

    [ObservableProperty]
    private ViewModelBase currentContent;

    [ObservableProperty]
    private WorkoutProgramViewModel? workoutProgram;

    [ObservableProperty]
    private ExerciseViewModel? exercises;

    [ObservableProperty]
    private GoalViewModel? goals;

    [ObservableProperty]
    private PersonalRecordViewModel? personalRecords;

    [ObservableProperty]
    private SupplementViewModel? supplements;

    [ObservableProperty]
    private SupplementIntakeViewModel? supplementSchedule;

    [ObservableProperty]
    private NutritionViewModel? nutrition;

    [ObservableProperty]
    private UserViewModel? profile;

    public MainViewModel()
    {
        _loginViewModel = new LoginViewModel(_authService);
        _loginViewModel.LoginSuccessful += OnLoginSuccessful;
        currentContent = _loginViewModel;
        _ = RestoreSessionAsync();
    }

    private async Task RestoreSessionAsync()
    {
        try
        {
            FirebaseService.Initialize();
            if (await _authService.TryAutoLoginAsync())
                await OpenWorkspaceAsync();
            else if (!string.IsNullOrWhiteSpace(_authService.LastErrorMessage))
                _loginViewModel.ValidationMessage = _authService.LastErrorMessage;
        }
        catch
        {
            _loginViewModel.ValidationMessage = "Couldn't connect to Firebase. Check your internet connection and try again.";
        }
    }

    private async void OnLoginSuccessful()
    {
        try
        {
            await OpenWorkspaceAsync();
        }
        catch
        {
            _loginViewModel.ValidationMessage = "Signed in, but couldn't load your cloud data. Check your connection and try again.";
            CurrentContent = _loginViewModel;
        }
    }

    private async Task OpenWorkspaceAsync()
    {
        var userId = _authService.CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Firebase did not provide an account ID.");

        var exerciseService = new ExerciseService();
        Exercises = new ExerciseViewModel(exerciseService) { CurrentUserId = userId };
        Goals = new GoalViewModel(new GoalService(), exerciseService) { CurrentUserId = userId };
        Supplements = new SupplementViewModel(new SupplementService()) { CurrentUserId = userId };
        Nutrition = new NutritionViewModel();
        PersonalRecords = new PersonalRecordViewModel(new PersonalRecordService(), exerciseService) { CurrentUserId = userId };
        SupplementSchedule = new SupplementIntakeViewModel(new SupplementIntakeService(), new SupplementService()) { CurrentUserId = userId };
        WorkoutProgram = new WorkoutProgramViewModel(new WorkoutProgramService(), exerciseService) { CurrentUserId = userId };
        Profile = new UserViewModel(new UserService(), userId, _authService);

        Profile.LoggedOut += ReturnToLogin;
        Profile.AccountDeleted += ReturnToLogin;
        Profile.ProfileDataConsentChanged += Nutrition.SetProfileDataConsent;

        CurrentContent = this;
        await Profile.LoadUserAsync();
        Nutrition.SetProfileDataConsent(Profile.ProfileDataConsentGranted);

        await Task.WhenAll(
            Exercises.LoadExercisesAsync(userId),
            Goals.LoadGoalsAsync(userId),
            Goals.LoadExercisesAsync(),
            PersonalRecords.LoadRecordsAsync(userId),
            PersonalRecords.LoadExercisesAsync(),
            Supplements.LoadSupplementAsync(userId),
            SupplementSchedule.LoadSupplementsAsync(userId),
            SupplementSchedule.LoadSupplementIntakesAsync(userId),
            WorkoutProgram.LoadExercisesAsync());

        await WorkoutProgram.LoadProgramsAsync(userId);
    }

    private void ReturnToLogin()
    {
        _authService.Logout();
        CurrentContent = _loginViewModel;
        WorkoutProgram = null;
        Exercises = null;
        Goals = null;
        PersonalRecords = null;
        Supplements = null;
        SupplementSchedule = null;
        Nutrition = null;
        Profile = null;
    }
}
