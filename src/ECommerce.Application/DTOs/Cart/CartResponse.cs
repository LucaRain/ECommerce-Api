namespace ECommerce.Application.DTOs.Cart;

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => UnitPrice * Quantity;
    public int AvailableStock { get; set; }
}
