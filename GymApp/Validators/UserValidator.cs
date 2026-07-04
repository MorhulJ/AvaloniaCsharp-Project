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
            .GreaterThan(0).WithMessage("Height must be greater than 0");
        
        RuleFor(u => u.weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");
    }
}