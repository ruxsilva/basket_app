using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Application.Mappers;
using BasketService.Domain.Models;
using Moq;

namespace BasketService.Tests.Unit.Application.Mappers;

public class MappingExtensionsTests
{
    [Fact]
    public async Task ToBasketDomainModelAsync_ShouldMapCorrectly()
    {
        // Arrange
        var basketDto = new BasketDto
        {
            Items =
            [
                new BasketItemDto { Name = "Soup", Quantity = 2 },
                new BasketItemDto { Name = "Bread", Quantity = 1 }
            ]
        };
            
        var mockRepo = new Mock<IBasketItemRepository>();
        mockRepo.Setup(r => r.GetItemByNameAsync("Soup"))
            .ReturnsAsync(new Item("Soup", 0.65m));
        mockRepo.Setup(r => r.GetItemByNameAsync("Bread"))
            .ReturnsAsync(new Item("Bread", 0.80m));
                
        // Act
        var basket = await basketDto.ToBasketDomainModelAsync(mockRepo.Object);
            
        // Assert
        Assert.Equal(2, basket.Items.Count);
        Assert.Equal("Soup", basket.Items[0].Item.Name);
        Assert.Equal(2, basket.Items[0].Quantity);
        Assert.Equal("Bread", basket.Items[1].Item.Name);
        Assert.Equal(1, basket.Items[1].Quantity);
    }
        
    [Fact]
    public async Task ToBasketDomainModelAsync_WithNonExistingItem_ShouldSkipItem()
    {
        // Arrange
        var basketDto = new BasketDto
        {
            Items =
            [
                new BasketItemDto { Name = "Soup", Quantity = 2 },
                new BasketItemDto { Name = "Cheese", Quantity = 1 }
            ]
        };
            
        var mockRepo = new Mock<IBasketItemRepository>();
        mockRepo.Setup(r => r.GetItemByNameAsync("Soup"))
            .ReturnsAsync(new Item("Soup", 0.65m));
        mockRepo.Setup(r => r.GetItemByNameAsync("Cheese"))
            .ReturnsAsync((Item)null);
                
        // Act
        var basket = await basketDto.ToBasketDomainModelAsync(mockRepo.Object);
            
        // Assert
        Assert.Single(basket.Items);
        Assert.Equal("Soup", basket.Items[0].Item.Name);
        Assert.Equal(2, basket.Items[0].Quantity);
    }

    [Fact]
    public void ToReceiptDto_ShouldMapCorrectly()
    {
        // Arrange
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
            
        // Act
        var receiptDto = receipt.ToReceiptDto();
            
        // Assert
        Assert.Equal(2, receiptDto.Items.Count);
        Assert.Single(receiptDto.Discounts);
        
        Assert.Equal("Soup", receiptDto.Items[0].Name);
        Assert.Equal(0.65m, receiptDto.Items[0].Price);
        Assert.Equal(2, receiptDto.Items[0].Quantity);
        Assert.Equal(1.30m, receiptDto.Items[0].LineTotal);
        
        Assert.Equal("Bread", receiptDto.Discounts[0].Name);
        Assert.Equal(0.80m, receiptDto.Discounts[0].OriginalPrice);
        Assert.Equal(0.40m, receiptDto.Discounts[0].DiscountAmount);
        Assert.Equal("Half price bread with soup", receiptDto.Discounts[0].DiscountReason);
        
        Assert.Equal(2.10m, receiptDto.TotalBeforeDiscount);
        Assert.Equal(0.40m, receiptDto.TotalDiscount);
        Assert.Equal(1.70m, receiptDto.FinalTotal);
    }
}