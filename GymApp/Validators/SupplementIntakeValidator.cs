using System;
using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class SupplementIntakeValidator : AbstractValidator<SupplementIntake>
{
    public SupplementIntakeValidator()
    {
        RuleFor(si => si.SupplementFirebaseId)
            .NotEmpty().WithMessage("Supplement is required");

        RuleFor(si => si.Dosage)
            .Must(value => double.IsFinite(value) && value > 0)
            .WithMessage("Supplement dosage must be a finite number greater than 0");

        RuleFor(si => si.Date)
            .IsInEnum().WithMessage("Choose a valid day of the week");

        RuleFor(si => si.Time)
            .Must(value => value >= TimeSpan.Zero && value < TimeSpan.FromDays(1))
            .WithMessage("Choose a valid time of day");
    }
}
