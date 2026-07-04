using SnapDocs.Domain.Entities;

namespace SnapDocs.Web.Models;

public class CashDashboardViewModel
{
    public decimal TotalCashBalance { get; set; }
    public decimal TodayReceipts { get; set; }
    public decimal TodayPayments { get; set; }
    public decimal NetToday => TodayReceipts - TodayPayments;
    public List<CashAccountBalanceViewModel> Accounts { get; set; } = new();
    public List<CashTransaction> RecentTransactions { get; set; } = new();
}

public class CashAccountBalanceViewModel
{
    public CashAccount Account { get; set; } = default!;
    public decimal Receipts { get; set; }
    public decimal Payments { get; set; }
    public decimal Balance => Account.OpeningBalance + Receipts - Payments;
}

public class CashAccountCreateViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Cash";
    public string CurrencyCode { get; set; } = "EGP";
    public decimal OpeningBalance { get; set; }
    public bool IsDefault { get; set; }
}
