namespace BasketService.Domain.Models;

public class Item(string name, decimal price)
{
    public string Name { get; } = name;
    public decimal Price { get; } = price;
}