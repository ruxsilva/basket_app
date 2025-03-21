using BasketService.Domain.Models;

namespace BasketService.Application.Interfaces;

public interface IBasketHistoryRepository
{
    Task<BasketHistory> SaveBasketHistoryAsync(BasketHistory basketHistory);
    Task<PaginatedResult<BasketHistory>> GetUserBasketHistoryPagedAsync(int userId, int page, int pageSize);
}