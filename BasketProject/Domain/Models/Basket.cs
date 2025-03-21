namespace BasketService.Domain.Models;

public class Basket(IEnumerable<BasketItem> items)
{
    public IReadOnlyList<BasketItem> Items { get; } = new List<BasketItem>(items);
}