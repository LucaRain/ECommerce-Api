using ECommerce.Application.DTOs.Order;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(Guid userId, CreateOrderRequest request);
    Task<List<OrderResponse>> GetMyOrdersAsync(Guid userId);
    Task<OrderResponse?> GetByIdAsync(Guid orderId, Guid userId, string role);
    Task<List<OrderResponse>> GetAllAsync(); // admin only
    Task<OrderResponse> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
}
