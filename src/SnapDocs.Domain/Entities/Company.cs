using SnapDocs.Domain.Common;

namespace SnapDocs.Domain.Entities;

public class Company : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
    public string ThemeColor { get; set; } = "blue";
    public string? LogoUrl { get; set; }
    public ICollection<CompanyUser> Users { get; set; } = new List<CompanyUser>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<DocumentTemplateSetting> TemplateSettings { get; set; } = new List<DocumentTemplateSetting>();
    public CompanyBranding? Branding { get; set; }
}
