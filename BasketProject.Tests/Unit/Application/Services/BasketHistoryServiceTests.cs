using BasketService.Application.Interfaces;
using BasketService.Application.Services;
using BasketService.Domain.Models;
using Moq;

namespace BasketService.Tests.Unit.Application.Services
{
    public class BasketHistoryServiceTests
    {
        private readonly Mock<IBasketHistoryRepository> _mockRepository;
        private readonly BasketHistoryService _basketHistoryService;

        public BasketHistoryServiceTests()
        {
            _mockRepository = new Mock<IBasketHistoryRepository>();
            _basketHistoryService = new BasketHistoryService(_mockRepository.Object);
        }

        [Fact]
        public async Task SaveBasketHistoryAsync_ShouldCreateAndSaveBasketHistory()
        {
            // Arrange
            var userId = 1;
            
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
            
            var savedBasketHistory = new BasketHistory 
            { 
                Id = 1, 
                UserId = userId,
                TotalAmount = 2.10m,
                TotalDiscount = 0.40m,
                FinalAmount = 1.70m,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockRepository.Setup(r => r.SaveBasketHistoryAsync(It.IsAny<BasketHistory>()))
                .ReturnsAsync(savedBasketHistory);
            
            // Act
            var result = await _basketHistoryService.SaveBasketHistoryAsync(receipt, userId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(savedBasketHistory.Id, result.Id);
            Assert.Equal(savedBasketHistory.UserId, result.UserId);
            Assert.Equal(savedBasketHistory.TotalAmount, result.TotalAmount);
            Assert.Equal(savedBasketHistory.TotalDiscount, result.TotalDiscount);
            Assert.Equal(savedBasketHistory.FinalAmount, result.FinalAmount);
            
            _mockRepository.Verify(r => r.SaveBasketHistoryAsync(It.Is<BasketHistory>(bh => 
                bh.UserId == userId && 
                bh.TotalAmount == receipt.TotalBeforeDiscount &&
                bh.TotalDiscount == receipt.TotalDiscount &&
                bh.FinalAmount == receipt.FinalTotal &&
                bh.Items.Count == 2)), Times.Once);
        }
        
        [Fact]
        public async Task GetUserBasketHistoryPagedAsync_ShouldReturnPaginatedHistory()
        {
            // Arrange
            var userId = 1;
            var page = 2;
            var pageSize = 10;
            var totalCount = 25;
            
            var expectedItems = new List<BasketHistory>
            {
                new() { Id = 11, UserId = userId },
                new() { Id = 12, UserId = userId },
                new() { Id = 13, UserId = userId },
                new() { Id = 14, UserId = userId },
                new() { Id = 15, UserId = userId }
            };
            
            var expectedResult = new PaginatedResult<BasketHistory>
            {
                Items = expectedItems,
                TotalCount = totalCount
            };
            
            _mockRepository.Setup(r => r.GetUserBasketHistoryPagedAsync(userId, page, pageSize))
                .ReturnsAsync(expectedResult);
            
            // Act
            var result = await _basketHistoryService.GetUserBasketHistoryPagedAsync(userId, page, pageSize);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(5, result.Items.Count());
            Assert.Equal(11, result.Items.ElementAt(0).Id);
            Assert.Equal(15, result.Items.ElementAt(4).Id);
            
            _mockRepository.Verify(r => r.GetUserBasketHistoryPagedAsync(userId, page, pageSize), Times.Once);
        }
    }
}