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
            .GreaterThan(0).WithMessage("Supplement dosage must be greater than 0");
    }
}