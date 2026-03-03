namespace ECommerce.Application.DTOs.Review;

public class CreateReviewRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
