using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(Guid userId);
    Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<CartResponse> UpdateQuantityAsync(
        Guid userId,
        Guid cartItemId,
        UpdateCartItemRequest request
    );
    Task RemoveFromCartAsync(Guid userId, Guid cartItemId);
    Task ClearCartAsync(Guid userId);
    Task<bool> CheckoutAsync(Guid userId); // converts cart to order
}
