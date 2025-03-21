using BasketService.Domain.Constants;
using BasketService.Domain.Interfaces;
using BasketService.Domain.Models;

namespace BasketService.Domain.Strategies;

public class SoupBreadDiscountStrategy : IDiscountStrategy
{
    private const decimal BreadDiscountPercentage = 0.5m;

    public IEnumerable<DiscountedItem> ApplyDiscount(IEnumerable<BasketItem> items)
    {
        var discountedItems = new List<DiscountedItem>();
            
        var basketItems = items.ToList();
        
        var soupItem = basketItems.FirstOrDefault(i => i.Item.Name.Equals(StrategiesConstants.Soup, System.StringComparison.OrdinalIgnoreCase));
        var breadItem = basketItems.FirstOrDefault(i => i.Item.Name.Equals(StrategiesConstants.Bread, System.StringComparison.OrdinalIgnoreCase));

        if (soupItem == null || breadItem == null)
        {
            return discountedItems;
        }
        
        // Calculate how many bread items can be discounted
        var soupPairs = soupItem.Quantity / 2;
        var breadToDiscount = System.Math.Min(soupPairs, breadItem.Quantity);

        if (breadToDiscount <= 0)
        {
            return discountedItems;
        }
        
        var originalBreadPrice = breadItem.Item.Price * breadToDiscount;
        var discountAmount = originalBreadPrice * BreadDiscountPercentage;
                    
        discountedItems.Add(new DiscountedItem(
            breadItem.Item.Name,
            originalBreadPrice,
            discountAmount,
            string.Format(StrategiesConstants.SoupDiscountReason, breadToDiscount)));

        return discountedItems;
    }
}