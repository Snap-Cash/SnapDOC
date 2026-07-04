namespace SnapDocs.Application.DTOs.SaaS;

public class BillingDashboardDto
{
    public TenantUsageDto Usage { get; set; } = new();
    public List<PlanCardDto> Plans { get; set; } = new();
    public List<BillingInvoiceDto> Invoices { get; set; } = new();
    public decimal CurrentBalance { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
}
