namespace BasketService.Application.DTO;

public class BasketHistoryDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<BasketHistoryItemDto> Items { get; set; } = new List<BasketHistoryItemDto>();
}

public class BasketHistoryItemDto
{
    public string ItemName { get; set; }
    public decimal ItemPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}