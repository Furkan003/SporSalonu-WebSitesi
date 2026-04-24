using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NfaSporSalonu.Models;
using NfaSporSalonu.Models.DTOs;

namespace NfaSporSalonu.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly NfaSporSalonuDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(NfaSporSalonuDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// JWT token al — Mobil uygulama login endpoint'i.
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new ApiErrorResponse { StatusCode = 400, Message = "E-posta ve şifre zorunludur." });

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new ApiErrorResponse { StatusCode = 401, Message = "E-posta veya şifre hatalı." });

        // Brute-force kilidi kontrolü
        if (user.LockoutEndTime.HasValue && user.LockoutEndTime > DateTime.Now)
        {
            var remaining = (int)(user.LockoutEndTime.Value - DateTime.Now).TotalMinutes;
            return StatusCode(429, new ApiErrorResponse
            {
                StatusCode = 429,
                Message = $"Hesabınız kilitli. {remaining + 1} dakika sonra tekrar deneyin."
            });
        }

        // Şifre doğrulama
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Hatalı giriş sayacını artır
            user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEndTime = DateTime.Now.AddMinutes(15);
                user.FailedLoginAttempts = 0;
                await _context.SaveChangesAsync();
                return StatusCode(429, new ApiErrorResponse
                {
                    StatusCode = 429,
                    Message = "Çok fazla hatalı giriş. Hesabınız 15 dakika kilitlendi."
                });
            }

            await _context.SaveChangesAsync();
            return Unauthorized(new ApiErrorResponse { StatusCode = 401, Message = "E-posta veya şifre hatalı." });
        }

        // Hesap aktif değilse
        if (user.IsActive == false)
            return StatusCode(403, new ApiErrorResponse { StatusCode = 403, Message = "Hesabınız devre dışı bırakılmıştır." });

        // Başarılı giriş — sayacı sıfırla
        user.FailedLoginAttempts = 0;
        user.LockoutEndTime = null;
        await _context.SaveChangesAsync();

        // JWT token oluştur
        var token = GenerateJwtToken(user);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "480");

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserInfoDto
            {
                Id = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Role = user.Role?.RoleName,
                ProfileImageUrl = user.ProfileImageUrl
            }
        });
    }

    /// <summary>
    /// Token yenileme endpoint'i.
    /// POST /api/auth/refresh
    /// </summary>
    [HttpPost("refresh")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Refresh()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null || user.IsActive == false)
            return Unauthorized(new ApiErrorResponse { StatusCode = 401, Message = "Geçersiz kullanıcı." });

        var token = GenerateJwtToken(user);
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "480");

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserInfoDto
            {
                Id = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Role = user.Role?.RoleName,
                ProfileImageUrl = user.ProfileImageUrl
            }
        });
    }

    // ─── JWT Token Generator ───

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "480");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.Role != null)
            claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
