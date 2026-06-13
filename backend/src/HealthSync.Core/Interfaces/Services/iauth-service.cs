using HealthSync.Core.DTOs.Auth;

namespace HealthSync.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResultDto> RefreshTokenAsync(string refereshToken);
    Task RevokeRefreshTokenAsync(string userId);
    Task<UserInfoDto?> GetUserByIdAsync(string userId);
    Task<List<UserInfoDto>> GetAllUsersAsync();
    Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<bool> ToggleActivationAsync(Guid id, bool isActive);
}
