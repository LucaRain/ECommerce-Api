using ECommerce.Application.DTOs.Review;

namespace ECommerce.Application.DTOs.Product;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<ProductImageResponse> Images { get; set; } = [];
    public string? MainImageUrl { get; set; }

    public List<ReviewResponse> Reviews { get; set; } = [];
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
