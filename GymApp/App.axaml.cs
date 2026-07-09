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
using GymApp.Data;
using GymApp.Services;
using Microsoft.EntityFrameworkCore;

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
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

            SetTheme("Dark");

            using (var db = new AppDbContext())
            {
                db.Database.Migrate();
            }

            var authDb = new AppDbContext();
            var authService = new AuthService(authDb);

            var autoUser = await authService.TryAutoLoginAsync();

            if (autoUser == null)
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
        var exerciseService = new ExerciseService(new AppDbContext());
        var goalService = new GoalService(new AppDbContext());
        var supplementService = new SupplementService(new AppDbContext());
        var personalRecordService = new PersonalRecordService(new AppDbContext());
        var supplementIntakeService = new SupplementIntakeService(new AppDbContext());
        var workoutProgramService = new WorkoutProgramService(new AppDbContext());
        var userService = new UserService(new AppDbContext());

        var userId = authService.CurrentUser!.Id;

        var exerciseViewModel = new ExerciseViewModel(exerciseService);
        var goalViewModel = new GoalViewModel(goalService, exerciseService);
        var supplementViewModel = new SupplementViewModel(supplementService);
        var nutritionViewModel = new NutritionViewModel();
        var personalRecordViewModel = new PersonalRecordViewModel(personalRecordService, exerciseService);
        var supplementIntakeViewModel = new SupplementIntakeViewModel(supplementIntakeService, supplementService);
        var workoutProgramViewModel = new WorkoutProgramViewModel(workoutProgramService, exerciseService);
        var userViewModel = new UserViewModel(userService, userId, authService);

        goalViewModel.CurrentUserId = userId;
        personalRecordViewModel.CurrentUserId = userId;
        supplementIntakeViewModel.CurrentUserId = userId;
        workoutProgramViewModel.CurrentUserId = userId;

        await exerciseViewModel.LoadExercisesAsync();
        await goalViewModel.LoadGoalsAsync(userId);
        await goalViewModel.LoadExercisesAsync();
        await supplementViewModel.LoadSupplementAsync();
        await supplementIntakeViewModel.LoadSupplementIntakesAsync(userId);
        await workoutProgramViewModel.LoadProgramsAsync(userId);
        await userViewModel.LoadUserAsync();
        await personalRecordViewModel.LoadRecordsAsync(userId);
        await personalRecordViewModel.LoadExercisesAsync();
        await supplementIntakeViewModel.LoadSupplementsAsync();
        await workoutProgramViewModel.LoadExercisesAsync();

        var mainWindowViewModel = new MainWindowViewModel(
            exerciseViewModel, goalViewModel, supplementViewModel,
            nutritionViewModel, personalRecordViewModel,
            supplementIntakeViewModel, workoutProgramViewModel,
            userViewModel, authService);

        var mainWindow = new MainWindow { DataContext = mainWindowViewModel };

        mainWindowViewModel.LoggedOut += () =>
        {
            Dispatcher.UIThread.Post(async () =>
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