using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HealthSync.Core.DTOs.Auth;
using HealthSync.Core.Entities.Identity;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HealthSync.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork uow, PasswordHasher<ApplicationUser> passwordHasher, IConfiguration configuration)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.UserName!.ToLower() == request.Username.ToLower());
        if (user == null || !user.IsActive)
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid credentials" };

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid credentials" };

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _uow.Users.UpdateAsync(user);
        }

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _uow.SaveChangesAsync();

        return new AuthResultDto
        {
            Success = true,
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserInfo(user)
        };
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid role" };

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return new AuthResultDto { Success = false, ErrorMessage = "Password must be at least 6 characters" };
        if (!request.Password.Any(char.IsDigit))
            return new AuthResultDto { Success = false, ErrorMessage = "Password must contain at least one digit" };

        var existingUser = await _uow.Users.Query().FirstOrDefaultAsync(u => u.UserName == request.Username);
        if (existingUser != null)
            return new AuthResultDto { Success = false, ErrorMessage = "Username already exists" };

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = role,
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        return new AuthResultDto { Success = true, UserId = user.Id.ToString() };
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow || !user.IsActive)
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid or expired refresh token" };

        var token = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _uow.SaveChangesAsync();

        return new AuthResultDto
        {
            Success = true,
            AccessToken = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserInfo(user)
        };
    }

    public async Task RevokeRefreshTokenAsync(string userId)
    {
        var user = await _uow.Users.GetByIdAsync(Guid.Parse(userId));
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
    {
        var user = await _uow.Users.GetByIdAsync(Guid.Parse(userId));
        return user == null ? null : MapToUserInfo(user);
    }

    public async Task<List<UserInfoDto>> GetAllUsersAsync()
    {
        var users = await _uow.Users.GetAllAsync();
        return users.Select(MapToUserInfo).ToList();
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return false;

        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.Role != null && Enum.TryParse<UserRole>(dto.Role, true, out var role))
            user.Role = role;

        user.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActivationAsync(Guid id, bool isActive)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static UserInfoDto MapToUserInfo(ApplicationUser user)
    {
        return new UserInfoDto
        {
            Id = user.Id.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            IsActive = user.IsActive
        };
    }
}
