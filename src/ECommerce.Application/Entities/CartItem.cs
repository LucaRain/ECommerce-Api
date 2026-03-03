namespace ECommerce.Application.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
