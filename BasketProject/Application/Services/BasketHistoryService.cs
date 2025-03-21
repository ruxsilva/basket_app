using BasketService.Application.Interfaces;
using BasketService.Domain.Models;

namespace BasketService.Application.Services;

public class BasketHistoryService(IBasketHistoryRepository basketHistoryRepository) : IBasketHistoryService
{
    public async Task<BasketHistory> SaveBasketHistoryAsync(Receipt receipt, int userId)
    {
        var basketHistory = new BasketHistory
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = receipt.TotalBeforeDiscount,
            TotalDiscount = receipt.TotalDiscount,
            FinalAmount = receipt.FinalTotal,
            Items = receipt.Items.Select(item => new BasketHistoryItem
            {
                ItemName = item.Item.Name,
                ItemPrice = item.Item.Price,
                Quantity = item.Quantity,
                LineTotal = item.Item.Price * item.Quantity
            }).ToList()
        };

        return await basketHistoryRepository.SaveBasketHistoryAsync(basketHistory);
    }
    
    public async Task<PaginatedResult<BasketHistory>> GetUserBasketHistoryPagedAsync(int userId, int page, int pageSize)
    {
        return await basketHistoryRepository.GetUserBasketHistoryPagedAsync(userId, page, pageSize);
    }
}