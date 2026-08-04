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
            .GreaterThan(0).WithMessage("Target value must be greater than 0");
    }
}