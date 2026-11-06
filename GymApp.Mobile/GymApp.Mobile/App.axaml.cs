using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GymApp.Mobile.ViewModels;
using GymApp.Mobile.Views;

namespace GymApp.Mobile;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            singleView.MainView = new RootView { DataContext = new MainViewModel() };

        base.OnFrameworkInitializationCompleted();
    }
}
