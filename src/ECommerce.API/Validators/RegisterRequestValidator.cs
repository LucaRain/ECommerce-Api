using ECommerce.Application.DTOs.Auth;
using FluentValidation;

namespace ECommerce.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MinimumLength(2)
            .WithMessage("Full name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Full name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(72) // bcrypt max length
            .WithMessage("Password must not exceed 72 characters")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one number");
    }
}
