namespace ECommerce.Application.DTOs.Common;

public class PagedRequest
{
    private int _limit = 10;

    public int Page { get; set; } = 1;

    public int Limit
    {
        get => _limit;
        set => _limit = value > 50 ? 50 : value; // max 50 per page
    }

    public string? Search { get; set; }
}
