using BasketService.Application.Interfaces;
using BasketService.Domain.Models;
using Dapper;
using MySql.Data.MySqlClient;

namespace BasketService.Infrastructure.Repositories
{
    public class BasketItemRepository(IConfiguration configuration) : IBasketItemRepository
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
    
            var results = await connection.QueryAsync("SELECT name, price FROM items");
            return results.Select(r => new Item(r.name, r.price));
        }

        public async Task<Item?> GetItemByNameAsync(string name)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
    
            var result = await connection.QueryFirstOrDefaultAsync(
                "SELECT name, price FROM items WHERE name = @Name",
                new { Name = name });

            return result == null ? null : new Item(result.name, result.price);
        }
    }
}