using System.Security.Claims;
using ECommerce.Application.DTOs.Review;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/products/{productId}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        var reviews = await _reviewService.GetByProductAsync(productId);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Guid productId, CreateReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var review = await _reviewService.CreateAsync(productId, userId, request);
        return CreatedAtAction(nameof(GetByProduct), new { productId }, review);
    }

    [HttpDelete("{reviewId}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid productId, Guid reviewId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(ClaimTypes.Role)!.Value;
        await _reviewService.DeleteAsync(reviewId, userId, role);
        return NoContent();
    }
}
