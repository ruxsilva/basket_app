using System.Security.Claims;
using BasketService.Application.DTO;
using BasketService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            try
            {
                var user = await authService.RegisterAsync(registerDto);
                var token = authService.GenerateJwtToken(user);
                
                return Ok(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            try
            {
                var userDto = await authService.LoginAsync(loginDto);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
        
        [Authorize]
        [HttpGet("testauth")]
        public ActionResult<string> TestAuth()
        {
            return Ok($"Hello, {User.FindFirst(ClaimTypes.Email)?.Value}! You are authenticated.");
        }
    }
}