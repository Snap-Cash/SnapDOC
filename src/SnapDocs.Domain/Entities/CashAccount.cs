using SnapDocs.Domain.Common;

namespace SnapDocs.Domain.Entities;

public class CashAccount : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Cash"; // Cash / Bank / Wallet
    public string CurrencyCode { get; set; } = "EGP";
    public decimal OpeningBalance { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CashTransaction> Transactions { get; set; } = new();
}
