using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities.SaaS;

public class SubscriptionPayment : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? BillingInvoiceId { get; set; }
    public BillingInvoice? BillingInvoice { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
    public string Provider { get; set; } = "Manual";
    public string ReferenceNumber { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
}
