using System.Security.Claims;
using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Domain.Models;
using BasketService.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BasketService.Tests.Unit.Web.Controllers
{
    public class BasketControllerTests
    {
        [Fact]
        public async Task ProcessBasket_WithValidBasketAndClaim_ShouldReturnReceipt()
        {
            // Arrange
            var mockBasketRepository = new Mock<IBasketService>();
            var mockItemRepository = new Mock<IBasketItemRepository>();
            var mockBasketHistoryRepository = new Mock<IBasketHistoryService>();
            
            var controller = new BasketController(mockBasketRepository.Object, mockItemRepository.Object, mockBasketHistoryRepository.Object);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            
            var basketDto = new BasketDto
            {
                Items =
                [
                    new BasketItemDto { Name = "Soup", Quantity = 2 },
                    new BasketItemDto { Name = "Bread", Quantity = 1 }
                ]
            };
            
            mockItemRepository.Setup(r => r.GetItemByNameAsync("Soup"))
                .ReturnsAsync(new Item("Soup", 0.65m));
            mockItemRepository.Setup(r => r.GetItemByNameAsync("Bread"))
                .ReturnsAsync(new Item("Bread", 0.80m));
            
            var items = new List<BasketItem>
            {
                new(new Item("Soup", 0.65m), 2),
                new(new Item("Bread", 0.80m), 1)
            };
            
            var discounts = new List<DiscountedItem>
            {
                new("Bread", 0.80m, 0.40m, "Half price bread with soup")
            };
            
            var receipt = new Receipt(items, discounts, 2.10m, 0.40m);
            
            mockBasketRepository.Setup(p => p.ProcessBasket(It.IsAny<Basket>()))
                .Returns(receipt);
                
            mockBasketHistoryRepository.Setup(h => h.SaveBasketHistoryAsync(receipt, 1))
                .ReturnsAsync(new BasketHistory());
                
            // Act
            var result = await controller.ProcessBasket(basketDto);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var receiptDto = Assert.IsType<ReceiptDto>(okResult.Value);
            
            Assert.Equal(2, receiptDto.Items.Count);
            Assert.Single(receiptDto.Discounts);
            Assert.Equal(2.10m, receiptDto.TotalBeforeDiscount);
            Assert.Equal(0.40m, receiptDto.TotalDiscount);
            Assert.Equal(1.70m, receiptDto.FinalTotal);
        }

        [Fact]
        public async Task ProcessBasket_WithEmptyBasket_ShouldReturnBadRequest()
        {
            // Arrange
            var mockBasketRepository = new Mock<IBasketService>();
            var mockItemRepository = new Mock<IBasketItemRepository>();
            var mockBasketHistoryRepository = new Mock<IBasketHistoryService>();
            
            var controller = new BasketController(mockBasketRepository.Object, mockItemRepository.Object, mockBasketHistoryRepository.Object);
            
            var emptyBasketDto = new BasketDto
            {
                Items = new List<BasketItemDto>()
            };
                
            // Act
            var result = await controller.ProcessBasket(emptyBasketDto);
            
            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAvailableItems_ShouldReturnAllItemsAsDto()
        {
            // Arrange
            var mockBasketRepository = new Mock<IBasketService>();
            var mockItemRepository = new Mock<IBasketItemRepository>();
            
            var mockBasketHistoryRepository = new Mock<IBasketHistoryService>();
            
            var controller = new BasketController(mockBasketRepository.Object, mockItemRepository.Object, mockBasketHistoryRepository.Object);
            
            var items = new List<Item>
            {
                new("Soup", 0.65m),
                new("Bread", 0.80m),
                new("Milk", 1.30m),
                new("Apples", 1.00m)
            };
            
            mockItemRepository.Setup(r => r.GetAllItemsAsync())
                .ReturnsAsync(items);
                
            // Act
            var result = await controller.GetAvailableItems();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedItems = Assert.IsType<IEnumerable<ItemDto>>(okResult.Value, exactMatch: false);
            Assert.Equal(4, returnedItems.Count());
        }
    }
}