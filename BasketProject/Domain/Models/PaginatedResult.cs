namespace BasketService.Domain.Models;

public class PaginatedResult<T> where T : class
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
}