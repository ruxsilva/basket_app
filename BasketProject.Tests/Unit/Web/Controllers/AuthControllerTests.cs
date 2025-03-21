using System.Security.Claims;
using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Domain.Models;
using BasketService.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BasketService.Tests.Unit.Web.Controllers;

public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;
        
        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }
        
        [Fact]
        public async Task Register_WhenSuccessful_ShouldReturnOkWithUserDto()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            
            var user = new User
            {
                Id = 1,
                Email = registerDto.Email
            };
            
            var token = "test.jwt.token";
            
            _mockAuthService.Setup(s => s.RegisterAsync(registerDto))
                .ReturnsAsync(user);
            _mockAuthService.Setup(s => s.GenerateJwtToken(user))
                .Returns(token);
            
            // Act
            var result = await _controller.Register(registerDto);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            
            Assert.Equal(user.Id, returnValue.Id);
            Assert.Equal(user.Email, returnValue.Email);
            Assert.Equal(token, returnValue.Token);
            
            _mockAuthService.Verify(s => s.RegisterAsync(registerDto), Times.Once);
            _mockAuthService.Verify(s => s.GenerateJwtToken(user), Times.Once);
        }
        
        [Fact]
        public async Task Register_WhenExceptionThrown_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };
            
            _mockAuthService.Setup(s => s.RegisterAsync(registerDto))
                .ThrowsAsync(new Exception("Email already exists"));
            
            // Act
            var result = await _controller.Register(registerDto);
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Email already exists", badRequestResult.Value);
            
            _mockAuthService.Verify(s => s.RegisterAsync(registerDto), Times.Once);
        }
        
        [Fact]
        public async Task Login_WhenSuccessful_ShouldReturnOkWithUserDto()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Password = "Password123"
            };
            
            var userDto = new UserDto
            {
                Id = 1,
                Email = "test@example.com",
                Token = "test.jwt.token"
            };
            
            _mockAuthService.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync(userDto);
            
            // Act
            var result = await _controller.Login(loginDto);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            
            Assert.Equal(userDto.Id, returnValue.Id);
            Assert.Equal(userDto.Email, returnValue.Email);
            Assert.Equal(userDto.Token, returnValue.Token);
            
            _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
        }
        
        [Fact]
        public async Task Login_WhenExceptionThrown_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Password = "WrongPassword"
            };
            
            _mockAuthService.Setup(s => s.LoginAsync(loginDto))
                .ThrowsAsync(new Exception("Wrong password"));
            
            // Act
            var result = await _controller.Login(loginDto);
            
            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Wrong password", unauthorizedResult.Value);
            
            _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
        }
        
        [Fact]
        public void TestAuth_ShouldReturnOkWithMessage()
        {
            // Arrange - Set up a ClaimsPrincipal for the controller
            // User.Identity.Name is not set by default when in a unit test context.
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            // Set the User property on the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            
            // Act
            var result = _controller.TestAuth();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var message = Assert.IsType<string>(okResult.Value);
            Assert.Contains("authenticated", message.ToLower());
        }
    }