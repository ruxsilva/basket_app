using BasketService.Application.DTO;
using BasketService.Application.Services;
using BasketService.Domain.Models;
using BasketService.Infrastructure.Repositories;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BasketService.Tests.Integration.Web
{
    public class AuthServiceIntegrationTests : IDisposable
    {
        private const string _connectionString = "Server=localhost;Port=3306;Database=auth_test_db;User=basket_user;Password=basket_password";
        private readonly MySqlConnection _setupConnection;
        private readonly AuthService _authService;

        public AuthServiceIntegrationTests()
        {
            _setupConnection = new MySqlConnection(_connectionString.Replace("auth_test_db", ""));
            _setupConnection.Open();
            

            _setupConnection.Execute("DROP DATABASE IF EXISTS auth_test_db");
            _setupConnection.Execute("CREATE DATABASE auth_test_db");

            // Create configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["UseDatabase"] = "true",
                    ["ConnectionStrings:DefaultConnection"] = _connectionString,
                    ["JWT:Secret"] = "c98dc7a0e14e707627abab499c06338aa8b9968f8b5ab0e320408a643ecb3bca"
                })
                .Build();
            
            InitializeDatabase().Wait();
            
            var userRepository = new UserRepository(configuration);
            var passwordService = new PasswordService();
            _authService = new AuthService(userRepository, configuration, passwordService);
        }
        
        private static async Task InitializeDatabase()
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
        }
        
        [Fact]
        public async Task RegisterAndLogin_ShouldWorkEndToEnd()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "integration@test.com",
                Password = "Password123!"
            };
            
            var loginDto = new LoginDto
            {
                Email = "integration@test.com",
                Password = "Password123!"
            };
            
            // Act
            var user = await _authService.RegisterAsync(registerDto);
            
            // Assert
            Assert.NotNull(user);
            Assert.Equal(registerDto.Email, user.Email);
            
            // Act
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var userExists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM users WHERE email = @Email", 
                new { Email = registerDto.Email });
            
            var userDto = await _authService.LoginAsync(loginDto);
            
            // Assert
            Assert.Equal(1, userExists);
            Assert.NotNull(userDto);
            Assert.Equal(loginDto.Email, userDto.Email);
            Assert.NotNull(userDto.Token);
            Assert.NotEmpty(userDto.Token);
        }
        
        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "wrong@test.com",
                Password = "CorrectPassword123!"
            };
            
            var loginDto = new LoginDto
            {
                Email = "wrong@test.com",
                Password = "WrongPassword123!"
            };
            
            // Act
            await _authService.RegisterAsync(registerDto);
            
            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _authService.LoginAsync(loginDto));
                
            Assert.Equal("Wrong password", exception.Message);
        }
        
        [Fact]
        public Task GenerateToken_ShouldCreateValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "token@test.com"
            };
            
            // Act
            var token = _authService.GenerateJwtToken(user);
            
            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            var parts = token.Split('.');
            Assert.Equal(3, parts.Length);
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            // Clean up - drop test database
            _setupConnection.Execute("DROP DATABASE IF EXISTS auth_test_db");
            _setupConnection.Dispose();
        }
    }
}