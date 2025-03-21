using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using BasketService.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace BasketService.Application.Services;

public class AuthService(IUserRepository userRepository, IConfiguration configuration, IPasswordService passwordService)
    : IAuthService
{
    public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            if (await userRepository.EmailExistsAsync(registerDto.Email))
                throw new Exception("Email already exists");
                
            passwordService.CreatePasswordHash(registerDto.Password, out var passwordHash, out var passwordSalt);
            
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            
            return await userRepository.CreateUserAsync(user);
        }

        public async Task<UserDto> LoginAsync(LoginDto loginDto)
        {
            var user = await userRepository.GetUserByEmailAsync(loginDto.Email);
            
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!passwordService.VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Wrong password");
            }
            
            var token = GenerateJwtToken(user);
            
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Token = token
            };
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email)
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["JWT:Secret"] ?? "no_secret_key"));
                
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }