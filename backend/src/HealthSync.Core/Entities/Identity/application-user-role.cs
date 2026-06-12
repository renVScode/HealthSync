using Microsoft.AspNetCore.Identity;

namespace HealthSync.Core.Entities.Identity;

public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public virtual ApplicationUserRole User { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
}
