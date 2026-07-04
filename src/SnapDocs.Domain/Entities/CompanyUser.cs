using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities;

public class CompanyUser : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
    public bool CanCreateDocuments { get; set; }
    public bool CanApproveDocuments { get; set; }
    public bool CanCancelDocuments { get; set; }
    public bool CanManageCustomers { get; set; }
    public bool CanManageSettings { get; set; }
    public bool IsActive { get; set; } = true;
}
