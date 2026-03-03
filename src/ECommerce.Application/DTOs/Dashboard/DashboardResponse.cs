namespace ECommerce.Application.DTOs.Dashboard;

public class DashboardResponse
{
    // overall stats
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }

    // this month
    public decimal RevenueThisMonth { get; set; }
    public int OrdersThisMonth { get; set; }
    public int NewCustomersThisMonth { get; set; }

    // orders by status
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();

    // top selling products
    public List<TopProductResponse> TopSellingProducts { get; set; } = new();

    // recent orders
    public List<RecentOrderResponse> RecentOrders { get; set; } = new();
}

public class TopProductResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class RecentOrderResponse
{
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}
