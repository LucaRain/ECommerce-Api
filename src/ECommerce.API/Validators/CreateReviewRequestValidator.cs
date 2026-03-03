using ECommerce.Application.DTOs.Review;
using FluentValidation;

namespace ECommerce.API.Validators;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment is required")
            .MinimumLength(10)
            .WithMessage("Comment must be at least 10 characters")
            .MaximumLength(1000)
            .WithMessage("Comment must not exceed 1000 characters");
    }
}
