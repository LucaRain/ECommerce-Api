using ECommerce.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.API.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MinimumLength(2)
            .WithMessage("Product name must be at least 2 characters")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required");
    }
}
