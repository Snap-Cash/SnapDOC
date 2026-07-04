using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PlanCode { get; set; } = "FREE";
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trial;
    public DateTime TrialEndsAt { get; set; } = DateTime.UtcNow.AddDays(14);
    public int MonthlyDocumentLimit { get; set; } = 30;
    public int MonthlyDocumentCount { get; set; }
    public ICollection<Company> Companies { get; set; } = new List<Company>();
}
