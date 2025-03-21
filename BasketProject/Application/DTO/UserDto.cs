namespace BasketService.Application.DTO;

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
}