namespace HealthSync.Core.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // create, update, delete
    public string EntityType { get; set; } = string.Empty; // patient, appointment, etc.
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; }  //json serialized
    public string? NewValues { get; set; } //json serialized
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public Identity.ApplicationUser? User { get; set; }
}
