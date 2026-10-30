using System;
using FluentValidation;
using GymApp.Models;

namespace GymApp.Validators;

public class UserValidator : AbstractValidator<User>
{
    public  UserValidator()
    {
        RuleFor(u => u.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        
        RuleFor(u => u.height)
            .InclusiveBetween(100, 250).WithMessage("Height must be between 100 and 250 cm")
            .When(u => u.height != 0);
        
        RuleFor(u => u.weight)
            .InclusiveBetween(30, 300).WithMessage("Weight must be between 30 and 300 kg")
            .When(u => u.weight != 0);

        RuleFor(u => u.Gender)
            .Must(gender => string.IsNullOrWhiteSpace(gender) || gender is "Male" or "Female")
            .WithMessage("Choose a valid gender option");

        RuleFor(u => u.DateOfBirth)
            .Must(date => date == DateTime.MinValue
                || (date.Date >= new DateTime(1900, 1, 1) && date.Date <= DateTime.Today))
            .WithMessage("Date of birth must be between 1900 and today");
    }
}
