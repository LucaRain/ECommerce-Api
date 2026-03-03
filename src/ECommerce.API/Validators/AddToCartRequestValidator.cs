using ECommerce.Application.DTOs.Cart;
using FluentValidation;

namespace ECommerce.API.Validators;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product is required");

        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
