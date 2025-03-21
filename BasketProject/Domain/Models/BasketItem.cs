namespace BasketService.Domain.Models;

public class BasketItem(Item item, int quantity)
{
    public Item Item { get; } = item;
    public int Quantity { get; } = quantity;
}