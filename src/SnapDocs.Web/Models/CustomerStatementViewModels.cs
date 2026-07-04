using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models;

public class CustomerStatementFilterViewModel
{
    public Guid? CustomerId { get; set; }
    public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public bool IncludeDrafts { get; set; }
    public string? Notes { get; set; }
}

public class CustomerStatementLineViewModel
{
    public DateTime Date { get; set; } = DateTime.Today;
    public string TypeName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid? SourceDocumentId { get; set; }
}

public class CustomerStatementDetailsViewModel
{
    public Guid? SnapshotId { get; set; }
    public string StatementNumber { get; set; } = string.Empty;
    public string VerifyCode { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public Customer Customer { get; set; } = default!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
    public string BalanceDirection => ClosingBalance > 0 ? "مدين" : ClosingBalance < 0 ? "دائن" : "متزن";
    public string? Notes { get; set; }
    public bool IsSnapshot { get; set; }
    public List<CustomerStatementLineViewModel> Lines { get; set; } = new();
}
