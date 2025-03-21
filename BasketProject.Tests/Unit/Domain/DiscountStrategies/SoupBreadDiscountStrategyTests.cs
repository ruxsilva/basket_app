using BasketService.Domain.Models;
using BasketService.Domain.Strategies;

namespace BasketService.Tests.Unit.Domain.DiscountStrategies;

public class SoupBreadDiscountStrategyTests
{
    [Fact]
    public void ApplyDiscount_WithTwoSoupsAndBread_ShouldReturnHalfPriceBreadDiscount()
    {
        // Arrange
        var strategy = new SoupBreadDiscountStrategy();
        var items = new List<BasketItem>
        {
            new(new Item("Soup", 0.65m), 2),
            new(new Item("Bread", 0.80m), 1)
        };

        // Act
        var discounts = strategy.ApplyDiscount(items).ToList();

        // Assert
        Assert.Single(discounts);
        var discount = discounts[0];
        Assert.Equal("Bread", discount.ItemName);
        Assert.Equal(0.80m, discount.OriginalPrice);
        Assert.Equal(0.40m, discount.DiscountAmount);
        Assert.Contains("Half price bread", discount.DiscountReason);
    }

    [Fact]
    public void ApplyDiscount_WithFourSoupsAndTwoBread_ShouldReturnDiscountForTwoBreads()
    {
        // Arrange
        var strategy = new SoupBreadDiscountStrategy();
        var items = new List<BasketItem>
        {
            new(new Item("Soup", 0.65m), 4),
            new(new Item("Bread", 0.80m), 2)
        };

        // Act
        var discounts = strategy.ApplyDiscount(items).ToList();

        // Assert
        Assert.Single(discounts);
        var discount = discounts[0];
        Assert.Equal("Bread", discount.ItemName);
        Assert.Equal(1.60m, discount.OriginalPrice); // 2 breads
        Assert.Equal(0.80m, discount.DiscountAmount); // 50% off 2 breads
        Assert.Contains("2 loaf/loaves", discount.DiscountReason);
    }

    [Fact]
    public void ApplyDiscount_WithOneSoupAndBread_ShouldReturnNoDiscounts()
    {
        // Arrange
        var strategy = new SoupBreadDiscountStrategy();
        var items = new List<BasketItem>
        {
            new(new Item("Soup", 0.65m), 1),
            new(new Item("Bread", 0.80m), 1)
        };

        // Act
        var discounts = strategy.ApplyDiscount(items).ToList();

        // Assert
        Assert.Empty(discounts);
    }

    [Fact]
    public void ApplyDiscount_WithTwoSoupsNoBread_ShouldReturnNoDiscounts()
    {
        // Arrange
        var strategy = new SoupBreadDiscountStrategy();
        var items = new List<BasketItem>
        {
            new(new Item("Soup", 0.65m), 2)
        };

        // Act
        var discounts = strategy.ApplyDiscount(items).ToList();

        // Assert
        Assert.Empty(discounts);
    }
}