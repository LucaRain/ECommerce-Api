namespace ECommerce.Application.Entities;

public class Review
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}
