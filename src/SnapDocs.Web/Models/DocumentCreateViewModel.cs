using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models;

public class DocumentCreateViewModel
{
    public DocumentType Type { get; set; } = DocumentType.Invoice;
    public Guid? CustomerId { get; set; }
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; } = 14;
    public string? Notes { get; set; }
    public List<DocumentItemViewModel> Items { get; set; } = new() { new() };
}

public class DocumentItemViewModel
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}
