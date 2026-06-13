using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using GymApp.ViewModels;
using GymApp.Views;
using GymApp.Data;
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
            var exerciseViewModel = new ExerciseViewModel(exerciseService);

            desktop.MainWindow = new MainWindow
            {
                DataContext = exerciseViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}