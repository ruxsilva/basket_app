using Dapper;
using MySql.Data.MySqlClient;

namespace BasketService.Infrastructure.Data
{
    public class DbSetup(IConfiguration configuration) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeDatabaseAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task InitializeDatabaseAsync()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            await connection.ExecuteAsync("CREATE DATABASE IF NOT EXISTS basket_setup_test_db");
            
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS users (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    email VARCHAR(100) NOT NULL UNIQUE,
                    password_hash VARBINARY(512) NOT NULL,
                    password_salt VARBINARY(512) NOT NULL
                )");
            
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS items (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    price DECIMAL(10, 2) NOT NULL
                )");

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
            
            // Insert seed data if table is empty
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM items");
            
            if (count == 0)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO items (name, price) VALUES 
                    ('Soup', 0.65),
                    ('Bread', 0.80),
                    ('Milk', 1.30),
                    ('Apples', 1.00)");
            }
        }
    }
}