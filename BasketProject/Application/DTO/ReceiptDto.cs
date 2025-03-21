namespace BasketService.Application.DTO;

public class ReceiptDto
{
    public List<BasketItemDetailsDto> Items { get; set; } = [];
    public List<DiscountedItemDto> Discounts { get; set; } = [];
    public decimal TotalBeforeDiscount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalTotal { get; set; }
}
    
public class BasketItemDetailsDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
