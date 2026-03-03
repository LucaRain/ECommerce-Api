using ECommerce.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.API.Validators;

public class ProductPagedRequestValidator : AbstractValidator<ProductPagedRequest>
{
    public ProductPagedRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.Limit).GreaterThan(0).WithMessage("Limit must be greater than 0");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Min price cannot be negative")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0)
            .WithMessage("Max price must be greater than 0")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => x.MinPrice <= x.MaxPrice)
            .WithMessage("Min price cannot be greater than max price")
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);
    }
}
