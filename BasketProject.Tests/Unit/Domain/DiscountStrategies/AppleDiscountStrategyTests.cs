using BasketService.Domain.Models;
using BasketService.Domain.Strategies;

namespace BasketService.Tests.Unit.Domain.DiscountStrategies
{
    public class AppleDiscountStrategyTests
    {
        [Fact]
        public void ApplyDiscount_WithApples_ShouldReturnTenPercentDiscount()
        {
            // Arrange
            var strategy = new AppleDiscountStrategy();
            var items = new List<BasketItem>
            {
                new(new Item("Apples", 1.00m), 4)
            };

            // Act
            var discounts = strategy.ApplyDiscount(items).ToList();

            // Assert
            Assert.Single(discounts);
            var discount = discounts[0];
            Assert.Equal("Apples", discount.ItemName);
            Assert.Equal(4.00m, discount.OriginalPrice);
            Assert.Equal(0.40m, discount.DiscountAmount);
            Assert.Contains("10%", discount.DiscountReason);
        }

        [Fact]
        public void ApplyDiscount_WithoutApples_ShouldReturnNoDiscounts()
        {
            // Arrange
            var strategy = new AppleDiscountStrategy();
            var items = new List<BasketItem>
            {
                new(new Item("Soup", 0.65m), 2),
                new(new Item("Bread", 0.80m), 1)
            };

            // Act
            var discounts = strategy.ApplyDiscount(items).ToList();

            // Assert
            Assert.Empty(discounts);
        }
    }
}