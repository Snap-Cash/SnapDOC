using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models;

public class CustomerStudioViewModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal TotalInvoices { get; set; }
    public decimal TotalReceipts { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalDocuments { get; set; }
    public decimal TotalQuotations { get; set; }
    public decimal TotalStatements { get; set; }
    public decimal CurrentBalance => OpeningBalance + TotalInvoices - TotalReceipts;
    public decimal AvailableCredit => CreditLimit <= 0 ? 0 : CreditLimit - Math.Max(CurrentBalance, 0);
    public decimal CreditUsagePercent => CreditLimit <= 0 ? 0 : Math.Min(100, Math.Max(0, (Math.Max(CurrentBalance, 0) / CreditLimit) * 100));
    public decimal AverageInvoice { get; set; }
    public decimal CollectionRate => TotalInvoices <= 0 ? 0 : Math.Min(100, (TotalReceipts / TotalInvoices) * 100);
    public int DocumentsCount { get; set; }
    public int InvoiceCount { get; set; }
    public int QuotationCount { get; set; }
    public int StatementCount { get; set; }
    public int ReceiptCount { get; set; }
    public int PaymentCount { get; set; }
    public int DraftCount { get; set; }
    public int SentCount { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }
    public DateTime? LastDocumentDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public CustomerAgingViewModel Aging { get; set; } = new();
    public IReadOnlyList<CustomerDocumentRowViewModel> RecentDocuments { get; set; } = new List<CustomerDocumentRowViewModel>();
    public IReadOnlyList<CustomerDocumentRowViewModel> Invoices { get; set; } = new List<CustomerDocumentRowViewModel>();
    public IReadOnlyList<CustomerDocumentRowViewModel> Statements { get; set; } = new List<CustomerDocumentRowViewModel>();
    public IReadOnlyList<CustomerDocumentRowViewModel> Receipts { get; set; } = new List<CustomerDocumentRowViewModel>();
    public IReadOnlyList<CustomerTimelineItemViewModel> Timeline { get; set; } = new List<CustomerTimelineItemViewModel>();
    public IReadOnlyList<CustomerDocumentTypeSummaryViewModel> DocumentTypeSummary { get; set; } = new List<CustomerDocumentTypeSummaryViewModel>();
}

public class CustomerAgingViewModel
{
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90 { get; set; }
    public decimal Total => Current + Days1To30 + Days31To60 + Days61To90 + Over90;
}

public class CustomerDocumentTypeSummaryViewModel
{
    public DocumentType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📄";
    public int Count { get; set; }
    public decimal Total { get; set; }
}

public class CustomerDocumentRowViewModel
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance => Total - PaidAmount;
    public string? Notes { get; set; }
}

public class CustomerTimelineItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Tone { get; set; } = "primary";
    public string Icon { get; set; } = "•";
}
