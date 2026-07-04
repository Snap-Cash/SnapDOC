using SnapDocs.Domain.Common;

namespace SnapDocs.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = "🔔";
    public bool IsRead { get; set; }
    public string? Url { get; set; }
}
