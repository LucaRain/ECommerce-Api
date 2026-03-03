using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Entities;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponse> CreateAsync(Guid userId, CreateOrderRequest request)
    {
        if (!request.Items.Any())
            throw new BadRequestException("Order must contain at least one item");

        // validate all products exist and have enough stock
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        if (products.Count != request.Items.Count)
            throw new NotFoundException("One or more products not found");

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (product.Stock < item.Quantity)
                throw new BadRequestException($"Insufficient stock for product '{product.Name}'");
        }

        // create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Pending",
            OrderDate = DateTime.UtcNow,
            OrderItems =
            [
                .. request.Items.Select(i =>
                {
                    var product = products.First(p => p.Id == i.ProductId);
                    return new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = product.Price, // snapshot price
                    };
                }),
            ],
        };

        // calculate total
        order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

        // decrease stock
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            product.Stock -= item.Quantity;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetOrderResponseAsync(order.Id);
    }

    public async Task<List<OrderResponse>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _context
            .Orders.AsNoTracking() // no tracking since we won't update entities
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return [.. orders.Select(ToResponse)];
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid orderId, Guid userId, string role)
    {
        var order = await _context
            .Orders.AsNoTracking() // no tracking since we won't update entities
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return null;

        // customers can only see their own orders
        if (role != "Admin" && order.UserId != userId)
            return null;

        return ToResponse(order);
    }

    public async Task<List<OrderResponse>> GetAllAsync()
    {
        var orders = await _context
            .Orders.AsNoTracking() // no tracking since we won't update entities
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return [.. orders.Select(ToResponse)];
    }

    public async Task<OrderResponse> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request
    )
    {
        var validStatuses = new[] { "Pending", "Shipped", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(request.Status))
            throw new BadRequestException(
                $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}"
            );

        var order = await _context
            .Orders.Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new NotFoundException("Order not found");

        // prevent invalid status transitions
        if (order.Status == "Delivered")
            throw new BadRequestException("Cannot change status of a delivered order");

        if (order.Status == "Cancelled")
            throw new BadRequestException("Order is already cancelled");

        // restore stock if cancelling
        if (request.Status == "Cancelled")
        {
            var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
            var products = await _context
                .Products.Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in order.OrderItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.Stock += item.Quantity; //  restore stock
            }
        }

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        return await GetOrderResponseAsync(orderId);
    }

    private async Task<OrderResponse> GetOrderResponseAsync(Guid orderId)
    {
        var order = await _context
            .Orders.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .FirstAsync(o => o.Id == orderId);

        return ToResponse(order);
    }

    private static OrderResponse ToResponse(Order o) =>
        new()
        {
            Id = o.Id,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            OrderDate = o.OrderDate,
            CustomerEmail = o.User?.Email ?? string.Empty,
            Items =
            [
                .. o.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                }),
            ],
        };
}
