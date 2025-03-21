namespace BasketService.Domain.Models;

public class DiscountedItem(string itemName, decimal originalPrice, decimal discountAmount, string discountReason)
{
    public string ItemName { get; } = itemName;
    public decimal OriginalPrice { get; } = originalPrice;
    public decimal DiscountAmount { get; } = discountAmount;
    public string DiscountReason { get; } = discountReason;
}