namespace ECommerce.Application.DTOs.Review;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
