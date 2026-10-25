using System;
using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class RecordValidator : AbstractValidator<PersonalRecord>
{
    public RecordValidator()
    {
        RuleFor(r => r.ExerciseFirebaseId)
            .NotEmpty().WithMessage("Record exercise is required");
        
        RuleFor(r => r.Value)
            .Must(value => double.IsFinite(value) && value > 0)
            .WithMessage("Record value must be a finite number greater than 0");

        RuleFor(r => r.Date)
            .LessThanOrEqualTo(_ => DateTimeOffset.Now)
            .WithMessage("Record date cannot be in the future");
    }
}
