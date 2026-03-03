using ECommerce.Application.DTOs.Review;
using ECommerce.Application.Entities;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewResponse>> GetByProductAsync(Guid productId)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found");

        return await _context
            .Reviews.AsNoTracking() // no tracking since we won't update entities
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => ToResponse(r))
            .ToListAsync();
    }

    public async Task<ReviewResponse> CreateAsync(
        Guid productId,
        Guid userId,
        CreateReviewRequest request
    )
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found");

        // check if user actually ordered this product
        var hasPurchased = await _context
            .Orders.Where(o => o.UserId == userId && o.Status == "Delivered")
            .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == productId));

        if (!hasPurchased)
            throw new BadRequestException(
                "You can only review products you have purchased and received"
            );

        // check if already reviewed
        var alreadyReviewed = await _context.Reviews.AnyAsync(r =>
            r.ProductId == productId && r.UserId == userId
        );

        if (alreadyReviewed)
            throw new BadRequestException("You have already reviewed this product");

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var created = await _context
            .Reviews.Include(r => r.User)
            .FirstAsync(r => r.Id == review.Id);

        return ToResponse(created);
    }

    public async Task DeleteAsync(Guid reviewId, Guid userId, string role)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
            throw new NotFoundException("Review not found");

        // customer can only delete their own review, admin can delete any
        if (role != "Admin" && review.UserId != userId)
            throw new UnauthorizedException("You can only delete your own reviews");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    private static ReviewResponse ToResponse(Review r) =>
        new()
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CustomerName = r.User?.FullName ?? string.Empty,
            CreatedAt = r.CreatedAt,
        };
}
