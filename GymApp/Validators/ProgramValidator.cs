using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class ProgramValidator : AbstractValidator<WorkoutProgram>
{
    public ProgramValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Workout program name is required")
            .MaximumLength(100).WithMessage("Name must be under 100 characters");
    }
}