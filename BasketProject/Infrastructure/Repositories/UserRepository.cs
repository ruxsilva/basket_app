using BasketService.Application.Interfaces;
using BasketService.Domain.Models;
using Dapper;
using MySql.Data.MySqlClient;

namespace BasketService.Infrastructure.Repositories
{
    public class UserRepository(IConfiguration configuration) : IUserRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = "SELECT id, email, password_hash as PasswordHash, password_salt as PasswordSalt FROM users WHERE email = @Email";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = @"
                INSERT INTO users (email, password_hash, password_salt) 
                VALUES (@Email, @PasswordHash, @PasswordSalt);
                SELECT LAST_INSERT_ID();";
            
            var id = await connection.ExecuteScalarAsync<int>(sql, new 
            { 
                user.Email, 
                PasswordHash = user.PasswordHash, 
                PasswordSalt = user.PasswordSalt 
            });
            
            user.Id = id;
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            
            var sql = "SELECT COUNT(1) FROM users WHERE email = @Email";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }
    }
}