using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities.SaaS;

public class TenantSubscription : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionPlan? Plan { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndsAt { get; set; }
    public DateTime? TrialEndsAt { get; set; } = DateTime.UtcNow.AddDays(14);
    public bool AutoRenew { get; set; }
}
