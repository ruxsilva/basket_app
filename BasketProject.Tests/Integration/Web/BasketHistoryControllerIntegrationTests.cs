using BasketService.Application.Services;
using BasketService.Domain.Models;
using BasketService.Infrastructure.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BasketService.Tests.Integration.Web
{
    public class BasketHistoryControllerIntegrationTests : IDisposable
    {
        private readonly string _connectionString = "Server=localhost;Port=3306;Database=basket_history_test_db;User=basket_user;Password=basket_password";
        private readonly MySqlConnection _setupConnection;
        private readonly BasketHistoryService _service;

        public BasketHistoryControllerIntegrationTests()
        {
            _setupConnection = new MySqlConnection(_connectionString.Replace("basket_history_test_db", ""));
            _setupConnection.Open();
            
            _setupConnection.Execute("DROP DATABASE IF EXISTS basket_history_test_db");
            _setupConnection.Execute("CREATE DATABASE basket_history_test_db");
            
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString
                })
                .Build();
            
            InitializeDatabase().Wait();
            
            BasketHistoryRepository repository = new BasketHistoryRepository(configuration);
            _service = new BasketHistoryService(repository);
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
        public async Task SaveAndRetrieveBasketHistory_ShouldWorkEndToEnd()
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
            
            // Act 
            var savedHistory = await _service.SaveBasketHistoryAsync(receipt, userId);
            
            // Assert
            Assert.NotNull(savedHistory);
            Assert.Equal(userId, savedHistory.UserId);
            Assert.Equal(2.10m, savedHistory.TotalAmount);
            Assert.Equal(0.40m, savedHistory.TotalDiscount);
            Assert.Equal(1.70m, savedHistory.FinalAmount);
            
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Starting to hate Dapper
            // had to define all the fields otherwise computer says no
            var dbHistory = await connection.QueryFirstOrDefaultAsync<BasketHistory>(
                "SELECT id, user_id as UserId, created_at as CreatedAt, total_amount as TotalAmount, " +
                "total_discount as TotalDiscount, final_amount as FinalAmount " +
                "FROM basket_history WHERE id = @Id", 
                new { Id = savedHistory.Id });
                
            Assert.NotNull(dbHistory);
            Assert.Equal(savedHistory.Id, dbHistory.Id);
            Assert.Equal(userId, dbHistory.UserId);
            Assert.Equal(2.10m, dbHistory.TotalAmount);
            Assert.Equal(0.40m, dbHistory.TotalDiscount);
            Assert.Equal(1.70m, dbHistory.FinalAmount);
            
            var dbItems = await connection.QueryAsync<BasketHistoryItem>(
                "SELECT id, item_name as ItemName, quantity FROM basket_history_items WHERE basket_history_id = @HistoryId",
                new { HistoryId = savedHistory.Id });
                
            Assert.Equal(2, dbItems.Count());
            Assert.Contains(dbItems, i => i.ItemName == "Soup" && i.Quantity == 2);
            Assert.Contains(dbItems, i => i.ItemName == "Bread" && i.Quantity == 1);
        }
        
        [Fact]
        public async Task GetUserBasketHistoryPaged_ShouldReturnCorrectPage()
        {
            // Arrange
            var userId = 1;
            
            // Create 15 basket histories with timestamps going backwards
            for (var i = 1; i <= 15; i++)
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var timestamp = DateTime.UtcNow.AddDays(-i);
                
                var historyId = await connection.ExecuteScalarAsync<int>(@"
                    INSERT INTO basket_history (user_id, created_at, total_amount, total_discount, final_amount)
                    VALUES (@UserId, @CreatedAt, @Amount, @Discount, @Final);
                    SELECT LAST_INSERT_ID();",
                    new 
                    { 
                        UserId = userId, 
                        CreatedAt = timestamp,
                        Amount = i + 0.50m,
                        Discount = 0.50m,
                        Final = i
                    });
                    
                // Add some items to each history
                await connection.ExecuteAsync(@"
                    INSERT INTO basket_history_items 
                    (basket_history_id, item_name, item_price, quantity, line_total)
                    VALUES (@HistoryId, @Name, @Price, @Quantity, @LineTotal)",
                    new
                    {
                        HistoryId = historyId,
                        Name = $"Item{i}",
                        Price = 1.00m,
                        Quantity = i,
                        LineTotal = i * 1.00m
                    });
            }
            
            // Act
            var paginatedResult = await _service.GetUserBasketHistoryPagedAsync(userId, 2, 5);
            
            // Assert
            Assert.NotNull(paginatedResult);
            Assert.Equal(15, paginatedResult.TotalCount);
            Assert.Equal(5, paginatedResult.Items.Count());
            
            var items = paginatedResult.Items.ToList();
            for (var i = 0; i < 5; i++)
            {
                var expectedNumber = i + 6;
                var item = items[i];
                
                Assert.Equal(expectedNumber + 0.50m, item.TotalAmount);
                Assert.Single(item.Items);
                Assert.Equal($"Item{expectedNumber}", item.Items.First().ItemName);
                Assert.Equal(expectedNumber, item.Items.First().Quantity);
            }
        }

        public void Dispose()
        {
            // Clean up - drop test database
            _setupConnection.Execute("DROP DATABASE IF EXISTS basket_history_test_db");
            _setupConnection.Dispose();
        }
    }
}