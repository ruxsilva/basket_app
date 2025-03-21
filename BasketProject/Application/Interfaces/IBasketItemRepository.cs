using BasketService.Domain.Models;

namespace BasketService.Application.Interfaces;

public interface IBasketItemRepository
{
    Task<IEnumerable<Item>> GetAllItemsAsync();
    Task<Item?> GetItemByNameAsync(string name);
}