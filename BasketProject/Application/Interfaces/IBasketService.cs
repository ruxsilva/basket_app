using BasketService.Domain.Models;

namespace BasketService.Application.Interfaces;

public interface IBasketService
{
    Receipt ProcessBasket(Basket basket);
}
