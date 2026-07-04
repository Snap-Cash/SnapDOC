using SnapDocs.Domain.Common;

namespace SnapDocs.Domain.Entities.Identity;

public class PermissionDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
}
