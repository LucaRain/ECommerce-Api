using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize] // all cart endpoints require login
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());
        return Ok(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartRequest request)
    {
        var cart = await _cartService.AddToCartAsync(GetUserId(), request);
        return Ok(cart);
    }

    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateQuantity(Guid cartItemId, UpdateCartItemRequest request)
    {
        var cart = await _cartService.UpdateQuantityAsync(GetUserId(), cartItemId, request);
        return Ok(cart);
    }

    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
    {
        await _cartService.RemoveFromCartAsync(GetUserId(), cartItemId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(GetUserId());
        return NoContent();
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        await _cartService.CheckoutAsync(GetUserId());
        return Ok(new { message = "Order placed successfully" });
    }
}
