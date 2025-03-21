using BasketService.Domain.Constants;
using BasketService.Domain.Interfaces;
using BasketService.Domain.Models;

namespace BasketService.Domain.Strategies;

public class AppleDiscountStrategy : IDiscountStrategy
{
    private const decimal DiscountPercentage = 0.1m;

    public IEnumerable<DiscountedItem> ApplyDiscount(IEnumerable<BasketItem> items)
    {
        var discountedItems = new List<DiscountedItem>();
            
        foreach (var item in items)
        {
            if (!item.Item.Name.Equals(StrategiesConstants.Apples, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            var originalPrice = item.Item.Price * item.Quantity;
            var discountAmount = originalPrice * DiscountPercentage;
                    
            discountedItems.Add(new DiscountedItem(
                item.Item.Name,
                originalPrice,
                discountAmount,
                StrategiesConstants.ApplesDiscountReason));
        }
            
        return discountedItems;
    }
}