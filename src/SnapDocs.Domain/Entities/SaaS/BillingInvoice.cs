using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities.SaaS;

public class BillingInvoice : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan? Plan { get; set; }
    public string Number { get; set; } = string.Empty;
    public BillingInvoiceStatus Status { get; set; } = BillingInvoiceStatus.Issued;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
    public string Notes { get; set; } = string.Empty;
}
