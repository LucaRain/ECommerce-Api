using ECommerce.Application.DTOs.Category;
using FluentValidation;

namespace ECommerce.API.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MinimumLength(2)
            .WithMessage("Category name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Category name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");
    }
}
