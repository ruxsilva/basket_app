using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Application.Services;
using BasketService.Domain.Models;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BasketService.Tests.Unit.Application.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly AuthService _authService;
        
        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockConfiguration = new Mock<IConfiguration>();
        
            // Set up JWT secret consistently using indexer approach
            _mockConfiguration.Setup(c => c["JWT:Secret"])
                .Returns("c98dc7a0e14e707627abab499c06338aa8b9968f8b5ab0e320408a643ecb3bca");
        
            _authService = new AuthService(
                _mockUserRepository.Object, 
                _mockConfiguration.Object,
                _mockPasswordService.Object);
        }
        
        [Fact]
        public async Task RegisterAsync_WithNewUser_ShouldCreateUser()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            
            _mockUserRepository.Setup(r => r.EmailExistsAsync(registerDto.Email))
                .ReturnsAsync(false);
                
            _mockPasswordService.Setup(p => p.CreatePasswordHash(registerDto.Password, out passwordHash, out passwordSalt));
            
            _mockUserRepository.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => 
                {
                    user.Id = 1;
                    return user;
                });
            
            // Act
            var result = await _authService.RegisterAsync(registerDto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(registerDto.Email, result.Email);
            
            _mockPasswordService.Verify(p => p.CreatePasswordHash(
                registerDto.Password, 
                out It.Ref<byte[]>.IsAny, 
                out It.Ref<byte[]>.IsAny), 
                Times.Once);
                
            _mockUserRepository.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Once);
        }
        
        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "Password123"
            };
            
            _mockUserRepository.Setup(r => r.EmailExistsAsync(registerDto.Email))
                .ReturnsAsync(true);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _authService.RegisterAsync(registerDto));
                
            Assert.Equal("Email already exists", exception.Message);
            _mockUserRepository.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }
        
        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnUserWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
    
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = [1, 2, 3],
                PasswordSalt = [4, 5, 6]
            };
            
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
        
            _mockPasswordService.Setup(p => p.VerifyPasswordHash(
                    loginDto.Password, user.PasswordHash, user.PasswordSalt))
                .Returns(true);
    
            // Act
            var userDto = await _authService.LoginAsync(loginDto);
    
            // Assert
            Assert.NotNull(userDto);
            Assert.Equal(user.Id, userDto.Id);
            Assert.Equal(user.Email, userDto.Email);
            Assert.NotNull(userDto.Token);
            Assert.NotEmpty(userDto.Token);
    
            _mockPasswordService.Verify(p => p.VerifyPasswordHash(
                    loginDto.Password, user.PasswordHash, user.PasswordSalt), 
                Times.Once);
        }
        
        [Fact]
        public async Task LoginAsync_WithInvalidUsername_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Password = "Password123"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _authService.LoginAsync(loginDto));
                
            Assert.Equal("User not found", exception.Message);
        }
        
        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };
    
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = [1, 2, 3],
                PasswordSalt = [4, 5, 6]
            };
            
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
        
            _mockPasswordService.Setup(p => p.VerifyPasswordHash(
                    loginDto.Password, user.PasswordHash, user.PasswordSalt))
                .Returns(false);
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _authService.LoginAsync(loginDto));
        
            Assert.Equal("Wrong password", exception.Message);
    
            _mockPasswordService.Verify(p => p.VerifyPasswordHash(
                    loginDto.Password, user.PasswordHash, user.PasswordSalt), 
                Times.Once);
        }
        
            
        [Fact]
        public void GenerateJwtToken_ShouldReturnValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com"
            };
        
            // Act - use the authService instance already created in the constructor
            var token = _authService.GenerateJwtToken(user);
        
            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }
    }
}
