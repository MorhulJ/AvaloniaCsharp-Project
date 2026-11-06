using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using System;
using GymApp.Mobile.ViewModels;
using GymApp.Mobile.Views;

namespace GymApp.Mobile;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public void SetTheme(bool dark)
    {
        var palette = dark ? "StylesDark" : "StylesLight";
        Styles[1] = new StyleInclude(new Uri("avares://GymApp.Mobile/"))
        {
            Source = new Uri($"avares://GymApp.Mobile/Styles/{palette}.axaml")
        };
        RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            singleView.MainView = new RootView { DataContext = new MainViewModel() };

        base.OnFrameworkInitializationCompleted();
    }
}
