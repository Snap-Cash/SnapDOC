using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities.Identity;

public class RolePermission : BaseEntity
{
    public Guid TenantId { get; set; }
    public UserRole Role { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public bool IsAllowed { get; set; } = true;
}
