using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using GymApp.ViewModels;
using GymApp.Views;
using GymApp.Data;
using GymApp.Models;
using GymApp.Services;

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
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            var exerciseService = new ExerciseService(new AppDbContext());
            var goalService = new GoalService(new AppDbContext());

            var exerciseViewModel = new ExerciseViewModel(exerciseService);
            var goalViewModel = new GoalViewModel(goalService, exerciseService);

            exerciseViewModel.LoadExercisesAsync().GetAwaiter().GetResult();
            goalViewModel.LoadGoalsAsync().GetAwaiter().GetResult();
            goalViewModel.LoadExercisesAsync().GetAwaiter().GetResult();

            var mainWindowViewModel = new MainWindowViewModel(exerciseViewModel, goalViewModel);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}