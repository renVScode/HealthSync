using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HealthSync.Core.DTOs.Auth;
using HealthSync.Core.Entities.Identity;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HealthSync.Core.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !user.IsActive)
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid credentials" };

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid credentials" };

        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

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

        var existingUser = await _userManager.FindByNameAsync(request.Username);
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

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return new AuthResultDto { Success = false, ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)) };

        var roleName = role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new ApplicationRole(roleName));
        await _userManager.AddToRoleAsync(user, roleName);

        return new AuthResultDto { Success = true, UserId = user.Id.ToString() };
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow || !user.IsActive)
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid or expired refresh token" };

        var token = await GenerateJwtTokenAsync(user);
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

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
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user == null ? null : MapToUserInfo(user);
    }

    public async Task<List<UserInfoDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserInfoDto>();
        foreach (var user in users)
            result.Add(MapToUserInfo(user));
        return result;
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.Role != null && Enum.TryParse<UserRole>(dto.Role, true, out var role))
            user.Role = role;

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ToggleActivationAsync(Guid id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        return true;
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var roles = await _userManager.GetRolesAsync(user);
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
