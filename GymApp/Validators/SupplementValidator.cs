using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class SupplementValidator : AbstractValidator<Supplement>
{
    public SupplementValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("Supplement name is required")
            .MaximumLength(100).WithMessage("Supplement name must be under 100 characters");

        RuleFor(s => s.DosageUnit)
            .NotEmpty().WithMessage("Dosage unit is required");
    }
}