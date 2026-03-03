namespace ECommerce.Application.Entities;

public class Order
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered, Cancelled
    public decimal TotalAmount { get; set; }

    // Foreign key
    public Guid UserId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
