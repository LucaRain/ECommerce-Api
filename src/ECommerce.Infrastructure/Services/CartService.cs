using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IRedisService _redis;
    private readonly IOrderService _orderService;

    public CartService(AppDbContext context, IRedisService redis, IOrderService orderService)
    {
        _context = context;
        _redis = redis;
        _orderService = orderService;
    }

    private static string CartKey(Guid userId) => $"cart:{userId}";

    public async Task<CartResponse> GetCartAsync(Guid userId)
    {
        var items =
            await _redis.GetAsync<List<CartItemResponse>>(CartKey(userId))
            ?? new List<CartItemResponse>();

        // refresh product prices and stock from DB (in case they changed)
        if (items.Any())
        {
            var productIds = items.Select(i => i.ProductId).ToList();
            var products = await _context
                .Products.Include(p => p.Images)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    item.UnitPrice = product.Price;
                    item.AvailableStock = product.Stock;
                    item.ProductName = product.Name;
                    item.ProductImage =
                        product.Images.FirstOrDefault(i => i.IsMain)?.Url
                        ?? product.Images.FirstOrDefault()?.Url;
                }
            }

            // save refreshed data back to Redis
            await _redis.SetAsync(CartKey(userId), items);
        }

        return new CartResponse { Items = items };
    }

    public async Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        var product = await _context
            .Products.Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
            throw new NotFoundException("Product not found");

        if (product.Stock < request.Quantity)
            throw new BadRequestException($"Only {product.Stock} items available in stock");

        var items =
            await _redis.GetAsync<List<CartItemResponse>>(CartKey(userId))
            ?? new List<CartItemResponse>();

        var existingItem = items.FirstOrDefault(i => i.ProductId == request.ProductId);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (newQuantity > product.Stock)
                throw new BadRequestException($"Only {product.Stock} items available in stock");

            existingItem.Quantity = newQuantity;
        }
        else
        {
            items.Add(
                new CartItemResponse
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImage =
                        product.Images.FirstOrDefault(i => i.IsMain)?.Url
                        ?? product.Images.FirstOrDefault()?.Url,
                    UnitPrice = product.Price,
                    Quantity = request.Quantity,
                    AvailableStock = product.Stock,
                }
            );
        }

        await _redis.SetAsync(CartKey(userId), items);
        return new CartResponse { Items = items };
    }

    public async Task<CartResponse> UpdateQuantityAsync(
        Guid userId,
        Guid cartItemId,
        UpdateCartItemRequest request
    )
    {
        var items =
            await _redis.GetAsync<List<CartItemResponse>>(CartKey(userId))
            ?? new List<CartItemResponse>();

        var item = items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
            throw new NotFoundException("Cart item not found");

        var product = await _context.Products.FindAsync(item.ProductId);
        if (product == null)
            throw new NotFoundException("Product not found");

        if (request.Quantity > product.Stock)
            throw new BadRequestException($"Only {product.Stock} items available in stock");

        item.Quantity = request.Quantity;

        await _redis.SetAsync(CartKey(userId), items);
        return new CartResponse { Items = items };
    }

    public async Task RemoveFromCartAsync(Guid userId, Guid cartItemId)
    {
        var items =
            await _redis.GetAsync<List<CartItemResponse>>(CartKey(userId))
            ?? new List<CartItemResponse>();

        var item = items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
            throw new NotFoundException("Cart item not found");

        items.Remove(item);
        await _redis.SetAsync(CartKey(userId), items);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        await _redis.DeleteAsync(CartKey(userId));
    }

    public async Task<bool> CheckoutAsync(Guid userId)
    {
        var items =
            await _redis.GetAsync<List<CartItemResponse>>(CartKey(userId))
            ?? new List<CartItemResponse>();

        if (!items.Any())
            throw new BadRequestException("Your cart is empty");

        // validate stock before placing order
        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        foreach (var item in items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                throw new NotFoundException($"Product '{item.ProductName}' no longer exists");
            if (product.Stock < item.Quantity)
                throw new BadRequestException($"Insufficient stock for '{item.ProductName}'");
        }

        // convert cart to order
        await _orderService.CreateAsync(
            userId,
            new CreateOrderRequest
            {
                Items = items
                    .Select(i => new OrderItemRequest
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                    })
                    .ToList(),
            }
        );

        // clear cart after successful order
        await ClearCartAsync(userId);
        return true;
    }
}
