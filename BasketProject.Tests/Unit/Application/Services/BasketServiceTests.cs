using BasketService.Domain.Interfaces;
using BasketService.Domain.Models;
using Moq;

namespace BasketService.Tests.Unit.Application.Services
{
    public class BasketServiceTests
    {
        [Fact]
        public void ProcessBasket_WithMultipleDiscountStrategies_ShouldApplyAllDiscounts()
        {
            // Arrange
            var mockStrategy1 = new Mock<IDiscountStrategy>();
            var mockStrategy2 = new Mock<IDiscountStrategy>();
            
            var discountStrategies = new List<IDiscountStrategy>
            {
                mockStrategy1.Object,
                mockStrategy2.Object
            };
            
            var processor = new BasketService.Application.Services.BasketService(discountStrategies);
            
            var items = new List<BasketItem>
            {
                new(new Item("Soup", 0.65m), 2),
                new(new Item("Bread", 0.80m), 1),
                new(new Item("Apples", 1.00m), 3)
            };
            
            var basket = new Basket(items);
            
            mockStrategy1.Setup(s => s.ApplyDiscount(It.IsAny<IEnumerable<BasketItem>>()))
                .Returns(new List<DiscountedItem>
                {
                    new("Bread", 0.80m, 0.40m, "Half price bread with soup")
                });
                
            mockStrategy2.Setup(s => s.ApplyDiscount(It.IsAny<IEnumerable<BasketItem>>()))
                .Returns(new List<DiscountedItem>
                {
                    new("Apples", 3.00m, 0.30m, "10% off apples")
                });

            // Act
            var receipt = processor.ProcessBasket(basket);

            // Assert
            Assert.Equal(3, receipt.Items.Count);
            Assert.Equal(2, receipt.Discounts.Count);
            Assert.Equal(0.40m + 0.30m, receipt.TotalDiscount);
            Assert.Equal(5.10m, receipt.TotalBeforeDiscount);
            Assert.Equal(4.40m, receipt.FinalTotal);
            
            mockStrategy1.Verify(s => s.ApplyDiscount(It.IsAny<IEnumerable<BasketItem>>()), Times.Once);
            mockStrategy2.Verify(s => s.ApplyDiscount(It.IsAny<IEnumerable<BasketItem>>()), Times.Once);
        }

        [Fact]
        public void ProcessBasket_WithNoDiscounts_ShouldReturnReceiptWithNoDiscounts()
        {
            // Arrange
            var mockStrategy = new Mock<IDiscountStrategy>();
            var discountStrategies = new List<IDiscountStrategy> { mockStrategy.Object };
            
            var processor = new BasketService.Application.Services.BasketService(discountStrategies);
            
            var items = new List<BasketItem>
            {
                new(new Item("Milk", 1.30m), 2)
            };
            
            var basket = new Basket(items);
            
            mockStrategy.Setup(s => s.ApplyDiscount(It.IsAny<IEnumerable<BasketItem>>()))
                .Returns(new List<DiscountedItem>());

            // Act
            var receipt = processor.ProcessBasket(basket);

            // Assert
            Assert.Single(receipt.Items);
            Assert.Empty(receipt.Discounts);
            Assert.Equal(2.60m, receipt.TotalBeforeDiscount);
            Assert.Equal(0m, receipt.TotalDiscount);
            Assert.Equal(2.60m, receipt.FinalTotal);
            
            mockStrategy.Verify(s => s.ApplyDiscount(It.IsAny<IEnumerable<BasketItem>>()), Times.Once);
        }
    }
}
