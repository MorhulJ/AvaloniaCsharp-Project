using FluentValidation;
using GymApp.ViewModels;

namespace GymApp.Validators;

public class NutritionValidator : AbstractValidator<NutritionViewModel>
{
    public NutritionValidator()
    {
        RuleFor(n => n.Weight)
            .GreaterThan(0).WithMessage("Weight is required")
            .LessThan(300).WithMessage("Enter a valid weight");

        RuleFor(n => n.Height)
            .GreaterThan(0).WithMessage("Height is required")
            .LessThan(250).WithMessage("Enter a valid height");

        RuleFor(n => n.Age)
            .GreaterThan(0).WithMessage("Age is required")
            .LessThan(120).WithMessage("Enter a valid age");
    }
}