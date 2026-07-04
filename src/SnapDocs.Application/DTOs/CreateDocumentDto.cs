using SnapDocs.Domain.Enums;

namespace SnapDocs.Application.DTOs;

public class CreateDocumentDto
{
    public Guid CompanyId { get; set; }
    public Guid? CustomerId { get; set; }
    public DocumentType Type { get; set; } = DocumentType.Invoice;
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; } = 14;
    public string? Notes { get; set; }
    public List<DocumentItemInputDto> Items { get; set; } = new();
}
