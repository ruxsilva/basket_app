using BasketService.Application.Interfaces;
using BasketService.Domain.Interfaces;
using BasketService.Domain.Models;

namespace BasketService.Application.Services;

public class BasketService(IEnumerable<IDiscountStrategy> discountStrategies) : IBasketService
{
    public Receipt ProcessBasket(Basket basket)
    {
        var totalBeforeDiscount = basket.Items.Sum(i => i.Item.Price * i.Quantity);
            
        // Apply all discount strategies
        var allDiscounts = new List<DiscountedItem>();
        foreach (var strategy in discountStrategies)
        {
            var discounts = strategy.ApplyDiscount(basket.Items);
            allDiscounts.AddRange(discounts);
        }
        
        var totalDiscount = allDiscounts.Sum(d => d.DiscountAmount);
        
        return new Receipt(
            basket.Items,
            allDiscounts,
            totalBeforeDiscount,
            totalDiscount);
    }
}