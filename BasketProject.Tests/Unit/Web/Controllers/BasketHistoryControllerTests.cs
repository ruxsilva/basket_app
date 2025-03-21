using BasketService.Application.Interfaces;
using BasketService.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using BasketService.Application.DTO;
using BasketService.Web.Controllers;


namespace BasketService.Tests.Unit.Web.Controllers;

public class BasketHistoryControllerTests
{
    private readonly Mock<IBasketHistoryService> _mockBasketHistoryService;
    private readonly BasketHistoryController _controller;

    public BasketHistoryControllerTests()
    {
        _mockBasketHistoryService = new Mock<IBasketHistoryService>();
        _controller = new BasketHistoryController(_mockBasketHistoryService.Object);
            
        SetupUserIdentity();
    }
        
    private void SetupUserIdentity(int userId = 1)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
            
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetUserBasketHistory_WithValidParameters_ShouldReturnPaginatedHistory()
    {
        // Arrange
        var userId = 1;
        var page = 2;
        var pageSize = 10;
            
        var historyItems = new List<BasketHistory>
        {
            new()
            { 
                Id = 1, 
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                TotalAmount = 5.10m,
                TotalDiscount = 0.50m,
                FinalAmount = 4.60m,
                Items = new List<BasketHistoryItem>
                {
                    new() { ItemName = "Soup", ItemPrice = 0.65m, Quantity = 2, LineTotal = 1.30m },
                    new() { ItemName = "Bread", ItemPrice = 0.80m, Quantity = 1, LineTotal = 0.80m }
                }
            },
            new()
            { 
                Id = 2, 
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 2.10m,
                TotalDiscount = 0.40m,
                FinalAmount = 1.70m,
                Items = new List<BasketHistoryItem>
                {
                    new() { ItemName = "Milk", ItemPrice = 1.30m, Quantity = 1, LineTotal = 1.30m },
                    new() { ItemName = "Apples", ItemPrice = 1.00m, Quantity = 1, LineTotal = 1.00m }
                }
            }
        };
            
        var paginatedResult = new PaginatedResult<BasketHistory>
        {
            Items = historyItems,
            TotalCount = 25
        };
            
        _mockBasketHistoryService.Setup(s => s.GetUserBasketHistoryPagedAsync(userId, page, pageSize))
            .ReturnsAsync(paginatedResult);
            
        // Act
        var result = await _controller.GetUserBasketHistory(page, pageSize);
            
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PaginatedResponse<BasketHistoryDto>>(okResult.Value);
            
        Assert.Equal(2, response.Items.Count());
        Assert.Equal(page, response.PageNumber);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(25, response.TotalCount);
        Assert.Equal(3, response.TotalPages);
        Assert.True(response.HasPreviousPage);
        Assert.True(response.HasNextPage);
            
        var firstItem = response.Items.First();
        Assert.Equal(1, firstItem.Id);
        Assert.Equal(5.10m, firstItem.TotalAmount);
        Assert.Equal(0.50m, firstItem.TotalDiscount);
        Assert.Equal(4.60m, firstItem.FinalAmount);
        Assert.Equal(2, firstItem.Items.Count);
            
        _mockBasketHistoryService.Verify(s => s.GetUserBasketHistoryPagedAsync(userId, page, pageSize), Times.Once);
    }
        
    [Fact]
    public async Task GetUserBasketHistory_WithInvalidParameters_ShouldUseDefaultValues()
    {
        // Arrange
        var userId = 1;
        var invalidPage = -1;
        var invalidPageSize = 100;
            
        var paginatedResult = new PaginatedResult<BasketHistory>
        {
            Items = new List<BasketHistory>(),
            TotalCount = 0
        };
            
        _mockBasketHistoryService.Setup(s => s.GetUserBasketHistoryPagedAsync(
                userId, 1, 10))
            .ReturnsAsync(paginatedResult);
            
        // Act
        var result = await _controller.GetUserBasketHistory(invalidPage, invalidPageSize);
            
        // Assert
        Assert.IsType<OkObjectResult>(result);
            
        _mockBasketHistoryService.Verify(s => s.GetUserBasketHistoryPagedAsync(
            userId, 1, 10), Times.Once);
    }
        
    [Fact]
    public async Task GetUserBasketHistory_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
            
        // Act
        var result = await _controller.GetUserBasketHistory();
            
        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
        
    [Fact]
    public async Task GetUserBasketHistory_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        var errorMessage = "Database error";
        _mockBasketHistoryService.Setup(s => s.GetUserBasketHistoryPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception(errorMessage));
            
        // Act
        var result = await _controller.GetUserBasketHistory();
            
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }
        
    [Fact]
    public async Task GetUserBasketHistory_WithEmptyHistory_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyResult = new PaginatedResult<BasketHistory>
        {
            Items = new List<BasketHistory>(),
            TotalCount = 0
        };
            
        _mockBasketHistoryService.Setup(s => s.GetUserBasketHistoryPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(emptyResult);
            
        // Act
        var result = await _controller.GetUserBasketHistory();
            
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PaginatedResponse<BasketHistoryDto>>(okResult.Value);
            
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
        Assert.False(response.HasNextPage);
    }
}