using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models.Reports;

namespace SnapDocs.Web.Controllers;

public class ReportsController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;

    public ReportsController(SnapDocsDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, Guid? customerId, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var from = fromDate?.Date ?? new DateTime(today.Year, today.Month, 1);
        var to = toDate?.Date ?? today;
        if (to < from) (from, to) = (to, from);

        var model = await BuildReportAsync(from, to, customerId, cancellationToken);
        await LoadLookups(customerId, cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Print(DateTime? fromDate, DateTime? toDate, Guid? customerId, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var from = fromDate?.Date ?? new DateTime(today.Year, today.Month, 1);
        var to = toDate?.Date ?? today;
        if (to < from) (from, to) = (to, from);

        var model = await BuildReportAsync(from, to, customerId, cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> ExportCsv(DateTime? fromDate, DateTime? toDate, Guid? customerId, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var from = fromDate?.Date ?? new DateTime(today.Year, today.Month, 1);
        var to = toDate?.Date ?? today;
        if (to < from) (from, to) = (to, from);

        var model = await BuildReportAsync(from, to, customerId, cancellationToken);
        var csv = new StringBuilder();
        csv.AppendLine("Number,Customer,Date,Type,Status,Total,Paid,Remaining");
        foreach (var row in model.RecentDocuments)
        {
            csv.AppendLine($"{Escape(row.Number)},{Escape(row.CustomerName)},{row.DocumentDate:yyyy-MM-dd},{row.Type},{row.Status},{row.Total:0.00},{row.PaidAmount:0.00},{row.Remaining:0.00}");
        }

        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv", $"snapdocs-report-{from:yyyyMMdd}-{to:yyyyMMdd}.csv");
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }

    private async Task<ReportsIndexViewModel> BuildReportAsync(DateTime from, DateTime to, Guid? customerId, CancellationToken cancellationToken)
    {
        var toExclusive = to.AddDays(1);
        var documentsQuery = _db.Documents
            .AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.CompanyId == DemoCompanyId && x.DocumentDate >= from && x.DocumentDate < toExclusive);

        if (customerId.HasValue)
            documentsQuery = documentsQuery.Where(x => x.CustomerId == customerId.Value);

        var documents = await documentsQuery
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var cashTransactions = await _db.CashTransactions
            .AsNoTracking()
            .Include(x => x.CashAccount)
            .Where(x => x.CompanyId == DemoCompanyId && x.TransactionDate >= from && x.TransactionDate < toExclusive)
            .ToListAsync(cancellationToken);

        if (customerId.HasValue)
            cashTransactions = cashTransactions.Where(x => x.CustomerId == customerId.Value).ToList();

        var customersQuery = _db.Customers.AsNoTracking().Where(x => x.CompanyId == DemoCompanyId);
        if (customerId.HasValue)
            customersQuery = customersQuery.Where(x => x.Id == customerId.Value);
        var customers = await customersQuery.ToListAsync(cancellationToken);

        var invoiceDocs = documents.Where(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled).ToList();
        var receiptDocs = documents.Where(x => x.Type == DocumentType.ReceiptVoucher && x.Status != DocumentStatus.Cancelled).ToList();
        var paymentDocs = documents.Where(x => x.Type == DocumentType.PaymentVoucher && x.Status != DocumentStatus.Cancelled).ToList();
        var creditDocs = documents.Where(x => x.Type == DocumentType.CreditNote && x.Status != DocumentStatus.Cancelled).ToList();
        var debitDocs = documents.Where(x => x.Type == DocumentType.DebitNote && x.Status != DocumentStatus.Cancelled).ToList();

        var totalSales = invoiceDocs.Sum(x => x.Total) + debitDocs.Sum(x => x.Total);
        var totalCollections = receiptDocs.Sum(x => x.Total) + invoiceDocs.Sum(x => x.PaidAmount) + creditDocs.Sum(x => x.Total);
        var totalPayments = paymentDocs.Sum(x => x.Total);
        var outstanding = Math.Max(0, totalSales - totalCollections);
        var cashIn = cashTransactions.Sum(x => x.Debit);
        var cashOut = cashTransactions.Sum(x => x.Credit);
        var invoiceValues = invoiceDocs.Select(x => x.Total).ToList();

        var today = DateTime.Today;
        var openInvoices = invoiceDocs.Where(x => x.Total > x.PaidAmount).ToList();
        var aging = BuildAging(openInvoices, today);

        var customerRows = customers.Select(c =>
        {
            var docs = documents.Where(x => x.CustomerId == c.Id).ToList();
            var sales = docs.Where(x => (x.Type == DocumentType.Invoice || x.Type == DocumentType.DebitNote) && x.Status != DocumentStatus.Cancelled).Sum(x => x.Total);
            var collections = docs.Where(x => (x.Type == DocumentType.ReceiptVoucher || x.Type == DocumentType.CreditNote) && x.Status != DocumentStatus.Cancelled).Sum(x => x.Total)
                              + docs.Where(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled).Sum(x => x.PaidAmount);
            return new ReportCustomerBalanceRow
            {
                CustomerId = c.Id,
                CustomerCode = c.Code,
                CustomerName = c.Name,
                Sales = sales,
                Collections = collections,
                Balance = c.OpeningBalance + sales - collections,
                DocumentsCount = docs.Count
            };
        }).ToList();

        var typeMetrics = documents
            .GroupBy(x => x.Type)
            .Select(g => new ReportDocumentTypeMetric
            {
                Type = g.Key,
                Label = GetTypeLabel(g.Key),
                Icon = GetTypeIcon(g.Key),
                Count = g.Count(),
                Total = g.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var monthly = documents
            .GroupBy(x => new { x.DocumentDate.Year, x.DocumentDate.Month })
            .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
            .Select(g => new ReportMonthlyMetric
            {
                MonthLabel = $"{g.Key.Year}/{g.Key.Month:00}",
                Sales = g.Where(x => (x.Type == DocumentType.Invoice || x.Type == DocumentType.DebitNote) && x.Status != DocumentStatus.Cancelled).Sum(x => x.Total),
                Collections = g.Where(x => (x.Type == DocumentType.ReceiptVoucher || x.Type == DocumentType.CreditNote) && x.Status != DocumentStatus.Cancelled).Sum(x => x.Total)
                    + g.Where(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled).Sum(x => x.PaidAmount),
                DocumentsCount = g.Count()
            })
            .ToList();

        var dailyTrend = documents
            .GroupBy(x => x.DocumentDate.Date)
            .OrderBy(x => x.Key)
            .Select(g => new ReportDailyMetric
            {
                DayLabel = g.Key.ToString("dd/MM"),
                Sales = g.Where(x => (x.Type == DocumentType.Invoice || x.Type == DocumentType.DebitNote) && x.Status != DocumentStatus.Cancelled).Sum(x => x.Total),
                Collections = g.Where(x => (x.Type == DocumentType.ReceiptVoucher || x.Type == DocumentType.CreditNote) && x.Status != DocumentStatus.Cancelled).Sum(x => x.Total)
                    + g.Where(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled).Sum(x => x.PaidAmount)
            })
            .Take(31)
            .ToList();

        var paymentMethods = cashTransactions
            .GroupBy(x => string.IsNullOrWhiteSpace(x.PaymentMethod) ? "غير محدد" : x.PaymentMethod)
            .Select(g => new ReportPaymentMethodMetric
            {
                Method = g.Key,
                Count = g.Count(),
                Total = g.Sum(x => x.Debit + x.Credit)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var cashAccounts = cashTransactions
            .GroupBy(x => new { Name = x.CashAccount != null ? x.CashAccount.Name : "حساب نقدي", Type = x.CashAccount != null ? x.CashAccount.Type : "Cash" })
            .Select(g => new ReportCashAccountMetric
            {
                AccountName = g.Key.Name,
                AccountType = g.Key.Type,
                In = g.Sum(x => x.Debit),
                Out = g.Sum(x => x.Credit)
            })
            .OrderByDescending(x => x.Net)
            .ToList();

        return new ReportsIndexViewModel
        {
            FromDate = from,
            ToDate = to,
            CustomerId = customerId,
            CustomerName = customerRows.FirstOrDefault()?.CustomerName,
            TotalSales = totalSales,
            TotalCollections = totalCollections,
            TotalPayments = totalPayments,
            CashIn = cashIn,
            CashOut = cashOut,
            OutstandingBalance = outstanding,
            OverdueBalance = aging.Days1To30 + aging.Days31To60 + aging.Days61To90 + aging.Over90,
            AverageInvoiceValue = invoiceValues.Any() ? invoiceValues.Average() : 0,
            HighestInvoiceValue = invoiceValues.Any() ? invoiceValues.Max() : 0,
            DocumentsCount = documents.Count,
            CustomersCount = customers.Count,
            InvoicesCount = invoiceDocs.Count,
            PaidInvoicesCount = invoiceDocs.Count(x => x.Total <= x.PaidAmount),
            OpenInvoicesCount = invoiceDocs.Count(x => x.Total > x.PaidAmount),
            RecentDocuments = documents.Take(12).Select(x => new ReportDocumentRow
            {
                Id = x.Id,
                Number = x.Number,
                CustomerName = x.Customer?.Name ?? "بدون عميل",
                Type = x.Type,
                Status = x.Status,
                DocumentDate = x.DocumentDate,
                Total = x.Total,
                PaidAmount = x.PaidAmount
            }).ToList(),
            TopCustomerBalances = customerRows.OrderByDescending(x => x.Balance).Take(10).ToList(),
            TopCustomersBySales = customerRows.OrderByDescending(x => x.Sales).Take(10).ToList(),
            DocumentTypeMetrics = typeMetrics,
            MonthlySales = monthly,
            DailyTrend = dailyTrend,
            PaymentMethods = paymentMethods,
            CashAccounts = cashAccounts,
            Aging = aging
        };
    }

    private static ReportAgingSummary BuildAging(IEnumerable<Document> openInvoices, DateTime today)
    {
        var aging = new ReportAgingSummary();
        foreach (var invoice in openInvoices)
        {
            var remaining = invoice.Total - invoice.PaidAmount;
            var due = invoice.DueDate?.Date ?? invoice.DocumentDate.Date;
            var days = (today - due).Days;

            if (days <= 0) aging.Current += remaining;
            else if (days <= 30) aging.Days1To30 += remaining;
            else if (days <= 60) aging.Days31To60 += remaining;
            else if (days <= 90) aging.Days61To90 += remaining;
            else aging.Over90 += remaining;
        }
        return aging;
    }

    private async Task LoadLookups(Guid? customerId, CancellationToken cancellationToken)
    {
        var customers = await _db.Customers
            .AsNoTracking()
            .Where(x => x.CompanyId == DemoCompanyId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
    }

    private static string GetTypeLabel(DocumentType type) => type switch
    {
        DocumentType.Invoice => "فواتير البيع",
        DocumentType.Quotation => "عروض الأسعار",
        DocumentType.CustomerStatement => "كشوف الحساب",
        DocumentType.ReceiptVoucher => "سندات القبض",
        DocumentType.PaymentVoucher => "سندات الصرف",
        DocumentType.DeliveryNote => "أذونات التسليم",
        DocumentType.CreditNote => "إشعارات دائنة",
        DocumentType.DebitNote => "إشعارات مدينة",
        _ => "مستندات"
    };

    private static string GetTypeIcon(DocumentType type) => type switch
    {
        DocumentType.Invoice => "🧾",
        DocumentType.Quotation => "💬",
        DocumentType.CustomerStatement => "📋",
        DocumentType.ReceiptVoucher => "📥",
        DocumentType.PaymentVoucher => "📤",
        DocumentType.DeliveryNote => "📦",
        DocumentType.CreditNote => "🟢",
        DocumentType.DebitNote => "🔴",
        _ => "📄"
    };
}
