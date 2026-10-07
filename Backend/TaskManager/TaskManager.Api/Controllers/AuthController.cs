using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Text;

namespace TaskManager.Api.Controllers;

public record AuthRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
{
    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest req)
    {
        var email = req.Email.Trim().ToLower();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict("El email ya existe.");                       // 409

        var user = new User(email, BCrypt.Net.BCrypt.HashPassword(req.Password));
        db.Users.Add(user);
        return Ok(await CreateTokens(user));
    }

    // POST /api/auth/login  ← FALTABA
    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest req)
    {
        var email = req.Email.Trim().ToLower();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Email o contraseña incorrectos.");      // 401

        return Ok(await CreateTokens(user));
    }

    // POST /api/auth/refresh  ← FALTABA
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest req)
    {
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == Hash(req.RefreshToken));
        if (stored is null || !stored.IsActive)
            return Unauthorized("Sesión expirada.");                     // 401

        stored.Revoke();                                                 // rotación: el usado ya no sirve
        var user = await db.Users.FindAsync(stored.UserId);
        return Ok(await CreateTokens(user!));
    }

    // Crea el JWT (15 min) y el refresh token (7 días, se guarda solo su hash)
    private async Task<object> CreateTokens(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var jwt = new JwtSecurityToken(
            claims: [new Claim("sub", user.Id.ToString())],
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken(user.Id, Hash(refreshToken), DateTime.UtcNow.AddDays(7)));
        await db.SaveChangesAsync();

        return new { accessToken = new JwtSecurityTokenHandler().WriteToken(jwt), refreshToken };
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}