using BasketService.Domain.Models;

namespace BasketService.Tests.Unit.Domain.Models;

public class ReceiptTests
{
    [Fact]
    public void Receipt_Constructor_ShouldCreateReceiptWithCorrectProperties()
    {
        // Arrange
        var totalBeforeDiscount = 2.10m;
        var totalDiscount = 0.40m;
        
        var item1 = new Item("Soup", 0.65m);
        var item2 = new Item("Bread", 0.80m);
        
        var basketItems = new List<BasketItem>
        {
            new(item1, 2),
            new(item2, 1)
        };

        var discounts = new List<DiscountedItem>
        {
            new("Bread", 0.80m, 0.40m, "Half price bread with soup")
        };

        // Act
        var receipt = new Receipt(basketItems, discounts, totalBeforeDiscount, totalDiscount);

        // Assert
        Assert.Equal(2, receipt.Items.Count);
        Assert.Single(receipt.Discounts);
        Assert.Equal(totalBeforeDiscount, receipt.TotalBeforeDiscount);
        Assert.Equal(totalDiscount, receipt.TotalDiscount);
        Assert.Equal(totalBeforeDiscount - totalDiscount, receipt.FinalTotal);
    }
}