using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities.Identity;

public class AppUser : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Owner;
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}
