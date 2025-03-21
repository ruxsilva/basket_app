using BasketService.Domain.Models;

namespace BasketService.Domain.Interfaces;

public interface IDiscountStrategy
{
    IEnumerable<DiscountedItem> ApplyDiscount(IEnumerable<BasketItem> items);
}