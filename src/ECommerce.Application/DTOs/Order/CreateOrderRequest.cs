namespace ECommerce.Application.DTOs.Order;

public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = [];
}

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
