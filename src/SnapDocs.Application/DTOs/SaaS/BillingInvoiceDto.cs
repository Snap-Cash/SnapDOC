namespace SnapDocs.Application.DTOs.SaaS;

public class BillingInvoiceDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
}
