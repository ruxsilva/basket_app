using BasketService.Application.Services;

namespace BasketService.Tests.Unit.Application.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService = new();

    [Fact]
    public void CreatePasswordHash_ShouldGenerateUniqueHashAndSalt()
    {
        // Arrange
        var password = "Password123";
            
        // Act
        _passwordService.CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
            
        // Assert
        Assert.NotNull(passwordHash);
        Assert.NotNull(passwordSalt);
        Assert.NotEmpty(passwordHash);
        Assert.NotEmpty(passwordSalt);
    }
        
    [Fact]
    public void VerifyPasswordHash_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "Password123";
        _passwordService.CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
            
        // Act
        var result = _passwordService.VerifyPasswordHash(password, passwordHash, passwordSalt);
            
        // Assert
        Assert.True(result);
    }
        
    [Fact]
    public void VerifyPasswordHash_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "Password123";
        var wrongPassword = "WrongPassword";
        _passwordService.CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
            
        // Act
        var result = _passwordService.VerifyPasswordHash(wrongPassword, passwordHash, passwordSalt);
            
        // Assert
        Assert.False(result);
    }
}