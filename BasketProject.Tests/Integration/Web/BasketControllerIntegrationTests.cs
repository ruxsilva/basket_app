using System.Security.Claims;
using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Application.Services;
using BasketService.Domain.Interfaces;
using BasketService.Domain.Strategies;
using BasketService.Infrastructure.Repositories;
using BasketService.Web.Controllers;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BasketService.Tests.Integration.Web
{
    public class BasketControllerIntegrationTests : IDisposable
    {
        private readonly string _connectionString = "Server=localhost;Port=3306;Database=basket_controller_test_db;User=basket_user;Password=basket_password";
        private readonly MySqlConnection _setupConnection;
        private readonly BasketController _controller;

        public BasketControllerIntegrationTests()
        {
            _setupConnection = new MySqlConnection(_connectionString.Replace("basket_controller_test_db", ""));
            _setupConnection.Open();
            
            _setupConnection.Execute("DROP DATABASE IF EXISTS basket_controller_test_db");
            _setupConnection.Execute("CREATE DATABASE basket_controller_test_db");
            
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString
                })
                .Build();
            
            InitializeDatabase().Wait();
            
            List<IDiscountStrategy> discountStrategies =
            [
                new AppleDiscountStrategy(),
                new SoupBreadDiscountStrategy()
            ];
            IBasketItemRepository basketItemRepository = new BasketItemRepository(configuration);
            IBasketService basketService = new Application.Services.BasketService(discountStrategies);
            var basketHistoryRepository = new BasketHistoryRepository(configuration);
            IBasketHistoryService basketHistoryService = new BasketHistoryService(basketHistoryRepository);
            
            _controller = new BasketController(basketService, basketItemRepository, basketHistoryService);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1")
            };
            
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }
        
        private async Task InitializeDatabase()
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS users (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    email VARCHAR(100) NOT NULL UNIQUE,
                    password_hash VARBINARY(512) NOT NULL,
                    password_salt VARBINARY(512) NOT NULL
                )");
            
            await connection.ExecuteAsync(@"
                INSERT INTO users (id, email, password_hash, password_salt) 
                VALUES (1, 'test@example.com', X'1234', X'5678')");
            
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS items (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    price DECIMAL(10, 2) NOT NULL
                )");
            
            await connection.ExecuteAsync(@"
                INSERT INTO items (id, name, price) VALUES 
                (1, 'Soup', 1.99),
                (2, 'Bread', 0.80),
                (3, 'Milk', 1.30)");
            
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS basket_history (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    created_at DATETIME NOT NULL,
                    total_amount DECIMAL(10, 2) NOT NULL,
                    total_discount DECIMAL(10, 2) NOT NULL,
                    final_amount DECIMAL(10, 2) NOT NULL,
                    FOREIGN KEY (user_id) REFERENCES users(id)
                )");

            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS basket_history_items (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    basket_history_id INT NOT NULL,
                    item_name VARCHAR(100) NOT NULL,
                    item_price DECIMAL(10, 2) NOT NULL,
                    quantity INT NOT NULL,
                    line_total DECIMAL(10, 2) NOT NULL,
                    FOREIGN KEY (basket_history_id) REFERENCES basket_history(id)
                )");
        }
        
        [Fact]
        public async Task ProcessBasket_WithValidItems_ReturnsReceiptAndSavesToHistory()
        {
            // Arrange
            var basketDto = new BasketDto
            {
                Items =
                [
                    new BasketItemDto { Name = "Soup", Quantity = 2 }, // Soup
                    new BasketItemDto { Name = "Bread", Quantity = 1 }
                ]
            };
            
            // Act
            var actionResult = await _controller.ProcessBasket(basketDto);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var receiptDto = Assert.IsType<ReceiptDto>(okResult.Value);
            
            Assert.NotNull(receiptDto);
            Assert.Equal(4.78m, receiptDto.TotalBeforeDiscount);  // 2 soups at 1.99 + 1 bread at 0.80
            
            Assert.Equal(2, receiptDto.Items.Count);
            Assert.Contains(receiptDto.Items, i => i.Name == "Soup" && i.Price == 1.99m && i.Quantity == 2);
            Assert.Contains(receiptDto.Items, i => i.Name == "Bread" && i.Price == 0.80m && i.Quantity == 1);
            
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var historyCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM basket_history WHERE user_id = 1");
            Assert.Equal(1, historyCount);
            
            var history = await connection.QueryFirstAsync<dynamic>(
                "SELECT total_amount as TotalAmount, final_amount as FinalAmount FROM basket_history WHERE user_id = 1");
            Assert.Equal(4.78m, (decimal)history.TotalAmount);
            
            var historyItems = await connection.QueryAsync<dynamic>(
                "SELECT bhi.item_name, bhi.quantity FROM basket_history bh " +
                "JOIN basket_history_items bhi ON bh.id = bhi.basket_history_id " +
                "WHERE bh.user_id = 1");
                
            Assert.Equal(2, historyItems.Count());
            Assert.Contains(historyItems, i => i.item_name == "Soup" && i.quantity == 2);
            Assert.Contains(historyItems, i => i.item_name == "Bread" && i.quantity == 1);
        }
        
        [Fact]
        public async Task ProcessBasket_WithEmptyBasket_ReturnsBadRequest()
        {
            // Arrange
            var basketDto = new BasketDto
            {
                Items = new List<BasketItemDto>()
            };
            
            // Act
            var actionResult = await _controller.ProcessBasket(basketDto);
            
            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var historyCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM basket_history WHERE user_id = 1");
            Assert.Equal(0, historyCount);
        }
        
        [Fact]
        public async Task GetAvailableItems_ReturnsAllItems()
        {
            // Act
            var actionResult = await _controller.GetAvailableItems();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var items = Assert.IsAssignableFrom<IEnumerable<ItemDto>>(okResult.Value);
            
            Assert.Equal(3, items.Count());
            Assert.Contains(items, i => i.Name == "Soup" && i.Price == 1.99m);
            Assert.Contains(items, i => i.Name == "Bread" && i.Price == 0.80m);
            Assert.Contains(items, i => i.Name == "Milk" && i.Price == 1.30m);
        }

        public void Dispose()
        {
            // Clean up - drop test database
            _setupConnection.Execute("DROP DATABASE IF EXISTS basket_controller_test_db");
            _setupConnection.Dispose();
        }
    }
}