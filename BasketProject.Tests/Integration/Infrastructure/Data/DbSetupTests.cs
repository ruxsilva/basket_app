using BasketService.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;

namespace BasketService.Tests.Integration.Infrastructure.Data;

public class DbSetupTests : IDisposable
{
    private readonly string _connectionString = "Server=localhost;Port=3306;Database=basket_setup_test_db;User=basket_user;Password=basket_password";
    private readonly MySqlConnection _connection;
    private readonly IConfiguration _configuration;

    public DbSetupTests()
    {
        // Create and open connection for setup and teardown
        _connection = new MySqlConnection(_connectionString.Replace("basket_setup_test_db", ""));
        _connection.Open();

        // Create test database
        _connection.Execute("DROP DATABASE IF EXISTS basket_setup_test_db");
        _connection.Execute("CREATE DATABASE basket_setup_test_db");
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["UseDatabase"] = "true",
                ["ConnectionStrings:DefaultConnection"] = _connectionString
            }!)
            .Build();
    }

    [Fact]
    public async Task StartAsync_WithUseDatabaseTrue_ShouldCreateTableAndSeedData()
    {
        // Arrange
        var dbSetup = new DbSetup(_configuration);

        // Act
        await dbSetup.StartAsync(CancellationToken.None);
        await using var testConnection = new MySqlConnection(_connectionString);
        await testConnection.OpenAsync();
        
        // Assert
        var tableExists = await testConnection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'basket_setup_test_db' AND table_name = 'items'");
        Assert.Equal(1, tableExists);
        
        var itemCount = await testConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM items");
        Assert.Equal(4, itemCount);
        
        var soup = await testConnection.QueryFirstOrDefaultAsync("SELECT price FROM items WHERE name = 'Soup'");
        Assert.Equal(0.65m, (decimal)soup.price);
    }

    [Fact]
    public async Task InitializeDatabaseAsync_WhenCalledTwice_ShouldNotDuplicateData()
    {
        // Arrange
        var dbSetup = new DbSetup(_configuration);

        // Act - Call twice to simulate application restart
        await dbSetup.StartAsync(CancellationToken.None);
        await dbSetup.StartAsync(CancellationToken.None);

        // Assert - Check that data was not duplicated
        await using var testConnection = new MySqlConnection(_connectionString);
        await testConnection.OpenAsync();
            
        var itemCount = await testConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM items");
        Assert.Equal(4, itemCount);
    }

    public void Dispose()
    {
        // Clean up - drop test database
        _connection.Execute("DROP DATABASE IF EXISTS basket_setup_test_db");
        _connection.Dispose();
    }
}