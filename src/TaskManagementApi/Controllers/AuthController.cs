using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskManagementApi.Models;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Аутентификация пользователя и выдача JWT-токена
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Проверка логина и пароля согласно заданию (student / 12345)
        if (request.Username != "student" || request.Password != "12345")
        {
            return Unauthorized(new
            {
                statusCode = StatusCodes.Status401Unauthorized,
                message = "Неверный логин или пароль."
            });
        }

        var jwtSection = _configuration.GetSection("Jwt");
        var secretKey = jwtSection["Key"] ?? "SecretKeyForTaskManagementApiAuthenticationModule06Lab06_SuperSecure!";
        var issuer = jwtSection["Issuer"] ?? "TaskManagementApi";
        var audience = jwtSection["Audience"] ?? "TaskManagementClient";
        var expirationMinutes = int.TryParse(jwtSection["ExpiresInMinutes"], out var mins) ? mins : 60;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        // Формирование Claims согласно заданию (Name = student, Role = User)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            tokenType = "Bearer",
            expiresInMinutes = expirationMinutes,
            expiration = tokenDescriptor.Expires,
            claims = new
            {
                name = request.Username,
                role = "User"
            }
        });
    }
}
