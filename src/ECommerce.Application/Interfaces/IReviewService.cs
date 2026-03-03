using ECommerce.Application.DTOs.Review;

namespace ECommerce.Application.Interfaces;

public interface IReviewService
{
    Task<List<ReviewResponse>> GetByProductAsync(Guid productId);
    Task<ReviewResponse> CreateAsync(Guid productId, Guid userId, CreateReviewRequest request);
    Task DeleteAsync(Guid reviewId, Guid userId, string role);
}
