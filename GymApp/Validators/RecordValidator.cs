using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class RecordValidator : AbstractValidator<PersonalRecord>
{
    public RecordValidator()
    {
        RuleFor(r => r.ExerciseId)
            .GreaterThan(0).WithMessage("Record exercise is required");
        
        RuleFor(r => r.Value)
            .GreaterThan(0).WithMessage("Record value must be greater than 0");
    }
}