using SnapDocs.Domain.Common;

namespace SnapDocs.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string ActorName { get; set; } = "System";
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityNumber { get; set; }
    public string? Notes { get; set; }
}
