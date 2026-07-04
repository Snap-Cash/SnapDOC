using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models.Reports;

public class ReportsIndexViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public decimal TotalSales { get; set; }
    public decimal TotalCollections { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal NetCashFlow => TotalCollections - TotalPayments;
    public decimal OutstandingBalance { get; set; }
    public decimal OverdueBalance { get; set; }
    public decimal AverageInvoiceValue { get; set; }
    public decimal HighestInvoiceValue { get; set; }
    public decimal CashIn { get; set; }
    public decimal CashOut { get; set; }
    public decimal CashNet => CashIn - CashOut;
    public int DocumentsCount { get; set; }
    public int CustomersCount { get; set; }
    public int InvoicesCount { get; set; }
    public int PaidInvoicesCount { get; set; }
    public int OpenInvoicesCount { get; set; }
    public decimal CollectionRate => TotalSales <= 0 ? 0 : Math.Round((TotalCollections / TotalSales) * 100, 1);
    public decimal OverdueRate => OutstandingBalance <= 0 ? 0 : Math.Round((OverdueBalance / OutstandingBalance) * 100, 1);

    public List<ReportDocumentRow> RecentDocuments { get; set; } = new();
    public List<ReportCustomerBalanceRow> TopCustomerBalances { get; set; } = new();
    public List<ReportCustomerBalanceRow> TopCustomersBySales { get; set; } = new();
    public List<ReportDocumentTypeMetric> DocumentTypeMetrics { get; set; } = new();
    public List<ReportMonthlyMetric> MonthlySales { get; set; } = new();
    public List<ReportDailyMetric> DailyTrend { get; set; } = new();
    public List<ReportPaymentMethodMetric> PaymentMethods { get; set; } = new();
    public List<ReportCashAccountMetric> CashAccounts { get; set; } = new();
    public ReportAgingSummary Aging { get; set; } = new();
}

public class ReportDocumentRow
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; }
    public DateTime DocumentDate { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Remaining => Total - PaidAmount;
}

public class ReportCustomerBalanceRow
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public decimal Collections { get; set; }
    public decimal Balance { get; set; }
    public int DocumentsCount { get; set; }
}

public class ReportDocumentTypeMetric
{
    public DocumentType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Total { get; set; }
}

public class ReportMonthlyMetric
{
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public decimal Collections { get; set; }
    public int DocumentsCount { get; set; }
}

public class ReportDailyMetric
{
    public string DayLabel { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public decimal Collections { get; set; }
    public decimal MaxValue => Math.Max(Sales, Collections);
}

public class ReportPaymentMethodMetric
{
    public string Method { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class ReportCashAccountMetric
{
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal In { get; set; }
    public decimal Out { get; set; }
    public decimal Net => In - Out;
}

public class ReportAgingSummary
{
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90 { get; set; }
    public decimal Total => Current + Days1To30 + Days31To60 + Days61To90 + Over90;
}
