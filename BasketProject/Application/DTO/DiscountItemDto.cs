namespace BasketService.Application.DTO;

public class DiscountedItemDto
{
    public string Name { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public string DiscountReason { get; set; }
}
