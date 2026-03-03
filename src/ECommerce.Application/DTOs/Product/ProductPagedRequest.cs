using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.DTOs.Product;

public class ProductPagedRequest : PagedRequest
{
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } = "createdAt"; // createdAt, price, name
    public string? SortOrder { get; set; } = "desc"; // asc, desc
}
