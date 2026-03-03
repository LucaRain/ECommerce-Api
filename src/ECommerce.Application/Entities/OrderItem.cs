namespace ECommerce.Application.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // snapshot of price at time of order

    // Foreign keys
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
