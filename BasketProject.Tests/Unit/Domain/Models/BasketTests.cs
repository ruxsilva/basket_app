using BasketService.Domain.Models;

namespace BasketService.Tests.Unit.Domain.Models;

public class BasketTests
{
    [Fact]
    public void Basket_Constructor_ShouldCreateBasketWithItems()
    {
        // Arrange
        var item1 = new Item("Soup", 0.65m);
        var item2 = new Item("Bread", 0.80m);
        
        var basketItems = new List<BasketItem>
        {
            new(item1, 2),
            new(item2, 1)
        };

        // Act
        var basket = new Basket(basketItems);

        // Assert
        Assert.Equal(2, basket.Items.Count);
        Assert.Equal(item1, basket.Items[0].Item);
        Assert.Equal(2, basket.Items[0].Quantity);
        Assert.Equal(item2, basket.Items[1].Item);
        Assert.Equal(1, basket.Items[1].Quantity);
    }

    [Fact]
    public void Basket_Constructor_ShouldCreateCopyOfItems()
    {
        // Arrange
        var items = new List<BasketItem>
        {
            new(new Item("Soup", 0.65m), 2)
        };

        // Act
        var basket = new Basket(items);
            
        // Attempt to modify original list (should not affect basket)
        items.Add(new BasketItem(new Item("Bread", 0.80m), 1));

        // Assert
        Assert.Single(basket.Items);
        Assert.Equal("Soup", basket.Items[0].Item.Name);
    }
}