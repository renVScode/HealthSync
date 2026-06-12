using HealthSync.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace HealthSync.Core.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // navigation properties
    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
    public ICollection<InventoryTransaction>? InventoryTransactions { get; set; }
    public ICollection<AuditLog>? AuditLogs { get; set; }
}
