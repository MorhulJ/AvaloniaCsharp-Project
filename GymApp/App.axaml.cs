using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using GymApp.ViewModels;
using GymApp.Views;
using GymApp.Services;

namespace GymApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            SetTheme("Dark");

            FirebaseService.Initialize();

            var authService = new AuthService();
            var autoLoggedIn = await authService.TryAutoLoginAsync();

            if (!autoLoggedIn)
            {
                ShowLoginWindow(desktop, authService);
            }
            else
            {
                await ShowMainWindowAsync(desktop, authService);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowLoginWindow(IClassicDesktopStyleApplicationLifetime desktop, AuthService authService)
    {
        var loginVm = new LoginViewModel(authService);
        var loginWindow = new LoginWindow(loginVm);

        desktop.MainWindow = loginWindow;
        loginWindow.Show();

        loginVm.LoginSuccessful += async () =>
        {
            loginWindow.Hide();
            await ShowMainWindowAsync(desktop, authService);
            loginWindow.Close();
        };
    }

    private async Task ShowMainWindowAsync(IClassicDesktopStyleApplicationLifetime desktop, AuthService authService)
    {
        var exerciseService = new ExerciseService();
        var goalService = new GoalService();
        var supplementService = new SupplementService();
        var personalRecordService = new PersonalRecordService();
        var supplementIntakeService = new SupplementIntakeService();
        var workoutProgramService = new WorkoutProgramService();
        var userService = new UserService();

        var userId = authService.CurrentUserId!;

        var exerciseViewModel = new ExerciseViewModel(exerciseService);
        var goalViewModel = new GoalViewModel(goalService, exerciseService);
        var supplementViewModel = new SupplementViewModel(supplementService);
        var nutritionViewModel = new NutritionViewModel();
        var personalRecordViewModel = new PersonalRecordViewModel(personalRecordService, exerciseService);
        var supplementIntakeViewModel = new SupplementIntakeViewModel(supplementIntakeService, supplementService);
        var workoutProgramViewModel = new WorkoutProgramViewModel(workoutProgramService, exerciseService);
        var userViewModel = new UserViewModel(userService, userId, authService);

        exerciseViewModel.CurrentUserId = userId;
        goalViewModel.CurrentUserId = userId;
        supplementViewModel.CurrentUserId = userId;
        personalRecordViewModel.CurrentUserId = userId;
        supplementIntakeViewModel.CurrentUserId = userId;
        workoutProgramViewModel.CurrentUserId = userId;

        await exerciseViewModel.LoadExercisesAsync(userId);
        await goalViewModel.LoadGoalsAsync(userId);
        await goalViewModel.LoadExercisesAsync();
        await supplementViewModel.LoadSupplementAsync(userId);
        await supplementIntakeViewModel.LoadSupplementIntakesAsync(userId);
        await supplementIntakeViewModel.LoadSupplementsAsync(userId);
        await workoutProgramViewModel.LoadProgramsAsync(userId);
        await workoutProgramViewModel.LoadExercisesAsync();
        await userViewModel.LoadUserAsync();
        await personalRecordViewModel.LoadRecordsAsync(userId);
        await personalRecordViewModel.LoadExercisesAsync();

        var mainWindowViewModel = new MainWindowViewModel(
            exerciseViewModel, goalViewModel, supplementViewModel,
            nutritionViewModel, personalRecordViewModel,
            supplementIntakeViewModel, workoutProgramViewModel,
            userViewModel, authService);

        var mainWindow = new MainWindow { DataContext = mainWindowViewModel };

        mainWindowViewModel.LoggedOut += () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                mainWindow.Hide();
                ShowLoginWindow(desktop, authService);
                mainWindow.Close();
            });
        };

        desktop.MainWindow = mainWindow;
        mainWindow.Show();
    }

    public void SetTheme(string themeName)
    {
        var styles = Application.Current!.Styles;

        if (styles.Count > 1)
            styles.RemoveAt(styles.Count - 1);

        var uri = themeName == "Dark"
            ? "avares://GymApp/Styles/StylesDark.axaml"
            : "avares://GymApp/Styles/StylesLight.axaml";

        var newStyle = new StyleInclude(new Uri("avares://GymApp/"))
        {
            Source = new Uri(uri)
        };

        styles.Add(newStyle);
    }
}