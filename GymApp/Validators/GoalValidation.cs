using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class GoalValidator : AbstractValidator<Goal>
{
    public GoalValidator()
    {
        RuleFor(g => g.Title)
            .NotEmpty().WithMessage("Goal title is required")
            .MaximumLength(100).WithMessage("Goal title must be under 100 characters");
        
        RuleFor(g => g.ExerciseFirebaseId)
            .NotEmpty().WithMessage("Goal exercise is required");
        
        RuleFor(g => g.TargetValue)
            .Must(value => double.IsFinite(value) && value > 0)
            .WithMessage("Target value must be a finite number greater than 0");

        RuleFor(g => g.CurrentValue)
            .Must(value => double.IsFinite(value) && value >= 0)
            .WithMessage("Current value must be a finite number and cannot be negative");
    }
}
