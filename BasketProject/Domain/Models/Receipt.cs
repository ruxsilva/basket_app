namespace BasketService.Domain.Models;

public class Receipt(
    IEnumerable<BasketItem> items,
    IEnumerable<DiscountedItem> discounts,
    decimal totalBeforeDiscount,
    decimal totalDiscount)
{
    public IReadOnlyList<BasketItem> Items { get; } = new List<BasketItem>(items);
    public IReadOnlyList<DiscountedItem> Discounts { get; } = new List<DiscountedItem>(discounts);
    public decimal TotalBeforeDiscount { get; } = totalBeforeDiscount;
    public decimal TotalDiscount { get; } = totalDiscount;
    public decimal FinalTotal { get; } = totalBeforeDiscount - totalDiscount;
}