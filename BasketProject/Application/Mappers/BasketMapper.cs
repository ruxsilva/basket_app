using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Domain.Models;

namespace BasketService.Application.Mappers
{
    public static class BasketMapper
    {
        public static async Task<Basket> ToBasketDomainModelAsync(
            this BasketDto? basketDto, 
            IBasketItemRepository basketItemRepository)
        {
            var basketItems = new List<BasketItem>();

            if (basketDto == null)
            {
                return new Basket(basketItems);
            }
            
            foreach (var itemDto in basketDto.Items)
            {
                if (itemDto.Name == null)
                {
                    continue;
                }
                
                var item = await basketItemRepository.GetItemByNameAsync(itemDto.Name);
                
                if (item != null)
                {
                    basketItems.Add(new BasketItem(item, itemDto.Quantity));
                }
            }


            return new Basket(basketItems);
        }
        
        public static ReceiptDto ToReceiptDto(this Receipt receipt)
        {
            return new ReceiptDto
            {
                Items = receipt.Items.Select(i => new BasketItemDetailsDto
                {
                    Name = i.Item.Name,
                    Price = i.Item.Price,
                    Quantity = i.Quantity,
                    LineTotal = i.Item.Price * i.Quantity
                }).ToList(),
                
                Discounts = receipt.Discounts.Select(d => new DiscountedItemDto
                {
                    Name = d.ItemName,
                    OriginalPrice = d.OriginalPrice,
                    DiscountAmount = d.DiscountAmount,
                    DiscountReason = d.DiscountReason
                }).ToList(),
                
                TotalBeforeDiscount = receipt.TotalBeforeDiscount,
                TotalDiscount = receipt.TotalDiscount,
                FinalTotal = receipt.FinalTotal
            };
        }

        private static ItemDto ToItemDto(this Item item)
        {
            return new ItemDto
            {
                Name = item.Name,
                Price = item.Price
            };
        }
        
        public static IEnumerable<ItemDto> ToItemDtos(this IEnumerable<Item> items)
        {
            return items.Select(item => item.ToItemDto());
        }
    }
}