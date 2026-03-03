using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly IRedisService _redis;
    private const string DashboardCacheKey = "dashboard:stats";

    public DashboardService(AppDbContext context, IRedisService redis)
    {
        _context = context;
        _redis = redis;
    }

    public async Task<DashboardResponse> GetStatsAsync()
    {
        // check cache first
        var cached = await _redis.GetAsync<DashboardResponse>(DashboardCacheKey);
        if (cached != null)
            return cached;

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalRevenue = await _context
            .Orders.AsNoTracking() // no tracking since we won't update entities
            .Where(o => o.Status != "Cancelled")
            .SumAsync(o => o.TotalAmount);

        var totalOrders = await _context.Orders.AsNoTracking().CountAsync();

        var totalProducts = await _context.Products.AsNoTracking().CountAsync();

        var totalCustomers = await _context
            .Users.AsNoTracking()
            .CountAsync(u => u.Role == "Customer");

        var revenueThisMonth = await _context
            .Orders.AsNoTracking() // no tracking since we won't update entities
            .Where(o => o.Status != "Cancelled" && o.OrderDate >= startOfMonth)
            .SumAsync(o => o.TotalAmount);

        var ordersThisMonth = await _context
            .Orders.AsNoTracking()
            .CountAsync(o => o.OrderDate >= startOfMonth);

        var newCustomersThisMonth = await _context
            .Users.AsNoTracking()
            .CountAsync(u => u.Role == "Customer" && u.CreatedAt >= startOfMonth);

        var ordersByStatus = await _context
            .Orders.AsNoTracking() // no tracking since we won't update entities
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var topProducts = await _context
            .OrderItems.AsNoTracking() // no tracking since we won't update entities
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.Status != "Cancelled")
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new TopProductResponse
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(5)
            .ToListAsync();

        var recentOrders = await _context
            .Orders.AsNoTracking() // no tracking since we won't update entities
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderResponse
            {
                OrderId = o.Id,
                CustomerEmail = o.User.Email,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                OrderDate = o.OrderDate,
            })
            .ToListAsync();

        var result = new DashboardResponse
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            RevenueThisMonth = revenueThisMonth,
            OrdersThisMonth = ordersThisMonth,
            NewCustomersThisMonth = newCustomersThisMonth,
            OrdersByStatus = ordersByStatus.ToDictionary(x => x.Status.ToLower(), x => x.Count),
            TopSellingProducts = topProducts,
            RecentOrders = recentOrders,
        };

        await _redis.SetAsync(DashboardCacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}
