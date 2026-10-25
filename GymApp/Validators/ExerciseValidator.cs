using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class ExerciseValidator : AbstractValidator<Exercise>
{
    public ExerciseValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty().WithMessage("Exercise name is required")
            .MaximumLength(100).WithMessage("Exercise name must be under 100 characters");

        RuleFor(e => e.MuscleGroup)
            .NotEmpty().WithMessage("Muscle group is required")
            .MaximumLength(50).WithMessage("Muscle group cannot exceed 50 characters");

        RuleFor(e => e.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1,000 characters");
    }
}
