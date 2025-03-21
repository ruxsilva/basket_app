using BasketService.Application.DTO;
using BasketService.Domain.Models;

namespace BasketService.Application.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterDto registerDto);
    Task<UserDto> LoginAsync(LoginDto loginDto);
    string GenerateJwtToken(User user);
}