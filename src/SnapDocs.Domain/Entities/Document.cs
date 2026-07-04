using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities;

public class Document : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public string Number { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public string VerifyCode { get; set; } = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
    public List<DocumentItem> Items { get; set; } = new();
}
