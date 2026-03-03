namespace ECommerce.Application.DTOs.Common;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Limit);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
