using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using GymApp.ViewModels;
using GymApp.Views;
using GymApp.Data;
using GymApp.Models;
using GymApp.Services;
using Microsoft.EntityFrameworkCore;

namespace GymApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            SetTheme("Dark");
            
            using (var db = new AppDbContext())
            {
                db.Database.Migrate();
                
                if (!db.Users.Any())
                {
                    db.Users.Add(new User
                    {
                        Name = "Test User",
                        Gender = "Male",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        weight = 80,
                        height = 180
                    });
                    db.SaveChanges();
                }
            }

            var exerciseService = new ExerciseService(new AppDbContext());
            var goalService = new GoalService(new AppDbContext());
            var supplementService = new SupplementService(new AppDbContext());
            var personalRecordService = new PersonalRecordService(new AppDbContext());
            var supplementIntakeService = new SupplementIntakeService(new AppDbContext());
            var workoutProgramSerivce = new WorkoutProgramService(new AppDbContext());
            var userService = new UserService(new AppDbContext());

            var exerciseViewModel = new ExerciseViewModel(exerciseService);
            var goalViewModel = new GoalViewModel(goalService, exerciseService);
            var supplementViewModel = new SupplementViewModel(supplementService);
            var nutritionViewModel = new NutritionViewModel();
            var personalRecordViewModel = new PersonalRecordViewModel(personalRecordService, exerciseService);
            var supplementIntakeViewModel = new SupplementIntakeViewModel(supplementIntakeService, supplementService);
            var workoutProgramViewModel = new WorkoutProgramViewModel(workoutProgramSerivce, exerciseService);
            var userViewModel = new UserViewModel(userService);

            exerciseViewModel.LoadExercisesAsync().GetAwaiter().GetResult();
            goalViewModel.LoadGoalsAsync().GetAwaiter().GetResult();
            goalViewModel.LoadExercisesAsync().GetAwaiter().GetResult();
            supplementViewModel.LoadSupplementAsync().GetAwaiter().GetResult();
            supplementIntakeViewModel.LoadSupplementIntakesAsync().GetAwaiter().GetResult();
            workoutProgramViewModel.LoadProgramsAsync().GetAwaiter().GetResult();
            userViewModel.LoadUserAsync().GetAwaiter().GetResult();
            personalRecordViewModel.LoadRecordsAsync().GetAwaiter().GetResult();
            personalRecordViewModel.LoadExercisesAsync().GetAwaiter().GetResult();
            supplementIntakeViewModel.LoadSupplementsAsync().GetAwaiter().GetResult();
            workoutProgramViewModel.LoadExercisesAsync().GetAwaiter().GetResult();

            var mainWindowViewModel = new MainWindowViewModel(exerciseViewModel, goalViewModel, supplementViewModel, nutritionViewModel, personalRecordViewModel, supplementIntakeViewModel,  workoutProgramViewModel, userViewModel);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    public void SetTheme(string themeName)
    {
        var styles = Application.Current!.Styles;

        if (styles.Count > 1)
            styles.RemoveAt(styles.Count - 1);

        var uri = themeName == "Dark" ? "avares://GymApp/Styles/StylesDark.axaml" : "avares://GymApp/Styles/StylesLight.axaml";

        var newStyle = new StyleInclude(new Uri("avares://GymApp/"))
        {
            Source = new Uri(uri)
        };

        styles.Add(newStyle);
    }
}