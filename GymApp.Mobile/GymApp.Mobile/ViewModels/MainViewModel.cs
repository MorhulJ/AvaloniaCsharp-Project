using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymApp.Services;
using GymApp.Mobile.Reports;
using GymApp.ViewModels;

namespace GymApp.Mobile.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly LoginViewModel _loginViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeActionText))]
    private bool isDarkTheme = true;

    public string ThemeActionText => IsDarkTheme ? "Light theme" : "Dark theme";

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        if (Avalonia.Application.Current is App app)
            app.SetTheme(IsDarkTheme);
    }

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

    public MainViewModel(ISessionStore? sessionStore = null)
    {
        _authService = new AuthService(sessionStore);
        _loginViewModel = new LoginViewModel(_authService);
        _loginViewModel.LoginSuccessful += OnLoginSuccessful;
        currentContent = _loginViewModel;
        _ = RestoreSessionAsync();
    }

    private async Task RestoreSessionAsync()
    {
        _loginViewModel.IsRestoringSession = true;
        _loginViewModel.ValidationMessage = "Checking saved sign-in…";
        try
        {
            FirebaseService.Initialize();
            if (await _authService.TryAutoLoginAsync())
            {
                _loginViewModel.RememberMe = true;
                _loginViewModel.ValidationMessage = "";
                await OpenWorkspaceAsync();
            }
            else
                _loginViewModel.ValidationMessage = _authService.LastErrorMessage ?? "";
        }
        catch
        {
            _loginViewModel.ValidationMessage = "Couldn't connect to Firebase. Check your internet connection and try again.";
        }
        finally { _loginViewModel.IsRestoringSession = false; }
    }

    private async void OnLoginSuccessful()
    {
        try
        {
            SetStatusMessage(_loginViewModel.ValidationMessage);
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
        var exercises = Exercises = new ExerciseViewModel(exerciseService) { CurrentUserId = userId };
        var goals = Goals = new GoalViewModel(new GoalService(), exerciseService) { CurrentUserId = userId };
        var supplements = Supplements = new SupplementViewModel(new SupplementService()) { CurrentUserId = userId };
        var nutrition = Nutrition = new NutritionViewModel();
        var records = PersonalRecords = new PersonalRecordViewModel(new PersonalRecordService(), exerciseService) { CurrentUserId = userId };
        var schedule = SupplementSchedule = new SupplementIntakeViewModel(new SupplementIntakeService(), new SupplementService()) { CurrentUserId = userId };
        var workout = WorkoutProgram = new WorkoutProgramViewModel(new WorkoutProgramService(), exerciseService) { CurrentUserId = userId };
        var profile = Profile = new UserViewModel(new UserService(), userId, _authService);

        profile.LoggedOut += ReturnToLogin;
        profile.AccountDeleted += ReturnToLogin;
        profile.ProfileDataConsentChanged += nutrition.SetProfileDataConsent;

        CurrentContent = this;
        await profile.LoadUserAsync();
        if (!ReferenceEquals(Profile, profile)) return;
        nutrition.SetProfileDataConsent(profile.ProfileDataConsentGranted);

        await Task.WhenAll(
            exercises.RetryAsync(),
            goals.RetryAsync(),
            records.RetryAsync(),
            supplements.RetryAsync(),
            schedule.RetryAsync(),
            workout.RetryAsync());
    }

    private void ReturnToLogin()
    {
        _loginViewModel.ValidationMessage = "";
        ClearStatusMessage();
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

    public async Task CreateReportAsync(string path)
    {
        var userId = _authService.CurrentUserId ?? throw new InvalidOperationException("Sign in to export a report.");
        var goals = new GoalService().GetAllGoalsByUserAsync(userId);
        var records = new PersonalRecordService().GetAllRecordsByUserAsync(userId);
        var exercises = new ExerciseService().GetAllExercisesAsync(userId);
        var user = new UserService().GetUserByIdAsync(userId);
        await Task.WhenAll(goals, records, exercises, user);
        var name = (await user)?.Name ?? "GymApp user";
        await Task.Run(() =>
        {
            using var output = File.Create(path);
            MobileWorkoutReport.Write(output, name, goals.Result, records.Result, exercises.Result);
        });
        if (userId != _authService.CurrentUserId)
            throw new InvalidOperationException("The sign-in session changed while creating the report.");
    }
}
