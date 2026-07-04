using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities;

public class CashTransaction : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid CashAccountId { get; set; }
    public CashAccount? CashAccount { get; set; }
    public Guid? DocumentId { get; set; }
    public Document? Document { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public CashTransactionType Type { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.Today;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}
