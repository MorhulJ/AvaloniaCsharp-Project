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
            .NotEmpty().WithMessage("Muscle group is required");
    }
}