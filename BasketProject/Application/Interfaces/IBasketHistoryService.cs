using BasketService.Domain.Models;

namespace BasketService.Application.Interfaces;

public interface IBasketHistoryService
{
    Task<BasketHistory> SaveBasketHistoryAsync(Receipt receipt, int userId);
    Task<PaginatedResult<BasketHistory>> GetUserBasketHistoryPagedAsync(int userId, int page, int pageSize);

}