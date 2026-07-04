using System.Data;
using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class ProgramExerciseValidator : AbstractValidator<ProgramExercise>
{
    public ProgramExerciseValidator()
    {
        RuleFor(pe => pe.ExerciseId)
            .GreaterThan(0).WithMessage("Program exercise is required");

        RuleFor(pe => pe.Sets)
            .GreaterThan(0).WithMessage("Sets must be greater than 0");

        RuleFor(pe => pe.Reps)
            .GreaterThan(0).WithMessage("Reps must be greater than 0");
    }
}