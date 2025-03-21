namespace BasketService.Domain.Models;

public class BasketHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalAmount { get; set; }
    public ICollection<BasketHistoryItem> Items { get; set; } = new List<BasketHistoryItem>();
}

public class BasketHistoryItem
{
    public int Id { get; set; }
    public int BasketHistoryId { get; set; }
    public string ItemName { get; set; }
    public decimal ItemPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}