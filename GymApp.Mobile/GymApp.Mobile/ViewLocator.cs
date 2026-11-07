using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GymApp.Mobile.ViewModels;
using GymApp.Mobile.Views;
using GymApp.ViewModels;
using GymApp.Views;

namespace GymApp.Mobile;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is MainViewModel) return new MainView();
        if (param is LoginViewModel) return new LoginView();
        if (param is not ViewModelBase viewModel) return null;
        Control view = viewModel switch
        {
            WorkoutProgramViewModel => new WorkoutProgramView(),
            ExerciseViewModel => new ExerciseView(),
            GoalViewModel => new GoalView(),
            PersonalRecordViewModel => new PersonalRecordView(),
            SupplementViewModel => new SupplementView(),
            SupplementIntakeViewModel => new SupplementIntakeView(),
            NutritionViewModel => new NutritionView(),
            UserViewModel => new UserView(),
            _ => new TextBlock { Text = "This section is unavailable." }
        };
        return new FeaturePage(view, viewModel);
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
