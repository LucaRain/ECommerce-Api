namespace ECommerce.Application.Entities;

public class ProductImage
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; } = false;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public Guid ProductId { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
