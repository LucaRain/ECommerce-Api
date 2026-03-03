namespace ECommerce.Application.DTOs.Cart;

public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
