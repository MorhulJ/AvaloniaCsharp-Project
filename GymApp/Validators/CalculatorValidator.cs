using FluentValidation;
using GymApp.ViewModels;

namespace GymApp.Validators;

public class NutritionValidator : AbstractValidator<NutritionViewModel>
{
    public NutritionValidator()
    {
        RuleFor(n => n.Weight)
            .InclusiveBetween(30, 300).WithMessage("For this adult calculator, enter a weight from 30 to 300 kg.");

        RuleFor(n => n.Height)
            .InclusiveBetween(100, 250).WithMessage("Enter a height from 100 to 250 cm.");

        RuleFor(n => n.Age)
            .InclusiveBetween(18, 100).WithMessage("This calculator is for adults aged 18 to 100.");

        RuleFor(n => n.Gender)
            .Must((viewModel, gender) => viewModel.GenderOptions.Contains(gender))
            .WithMessage("Choose one of the available sex values for the formula.");

        RuleFor(n => n.ActivityLevel)
            .Must((viewModel, activity) => viewModel.ActivityLevelOptions.Contains(activity))
            .WithMessage("Choose an activity level.");

        RuleFor(n => n.Goal)
            .Must((viewModel, goal) => viewModel.GoalOptions.Contains(goal))
            .WithMessage("Choose a goal.");
    }
}
