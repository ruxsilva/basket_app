using BasketService.Domain.Models;

namespace BasketService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}