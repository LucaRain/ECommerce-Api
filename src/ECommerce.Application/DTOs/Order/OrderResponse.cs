namespace ECommerce.Application.DTOs.Order;

public class OrderResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItemResponse> Items { get; set; } = [];
}

public class OrderItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
