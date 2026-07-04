using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models.Dashboard;

namespace SnapDocs.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly SnapDocsDbContext _db;

    public DashboardController(SnapDocsDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = GetCompanyId();
        var tenantId = GetTenantId();
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken);
        var tenant = await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        var subscription = await _db.TenantSubscriptions
            .AsNoTracking()
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var documentsQuery = _db.Documents.AsNoTracking().Where(x => x.CompanyId == companyId);
        var monthDocumentsQuery = documentsQuery.Where(x => x.DocumentDate >= monthStart && x.DocumentDate <= today.AddDays(1));

        var totalDocuments = await documentsQuery.CountAsync(cancellationToken);
        var monthDocuments = await monthDocumentsQuery.CountAsync(cancellationToken);
        var totalCustomers = await _db.Customers.AsNoTracking().CountAsync(x => x.CompanyId == companyId, cancellationToken);
        var newCustomers = await _db.Customers.AsNoTracking().CountAsync(x => x.CompanyId == companyId && x.CreatedAtUtc >= monthStart, cancellationToken);

        var monthSales = await monthDocumentsQuery
            .Where(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled)
            .SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;

        var paid = await monthDocumentsQuery
            .Where(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled)
            .SumAsync(x => (decimal?)x.PaidAmount, cancellationToken) ?? 0m;

        var outstanding = await documentsQuery
            .Where(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled)
            .SumAsync(x => (decimal?)(x.Total - x.PaidAmount), cancellationToken) ?? 0m;

        var overdue = await documentsQuery
            .CountAsync(x => x.Type == DocumentType.Invoice && x.Status != DocumentStatus.Cancelled && x.DueDate != null && x.DueDate < today && x.Total > x.PaidAmount, cancellationToken);

        var recentDocs = await _db.Documents.AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(8)
            .Select(x => new DashboardDocumentRow
            {
                Id = x.Id,
                Number = x.Number,
                CustomerName = x.Customer != null ? x.Customer.Name : "بدون عميل",
                Type = x.Type,
                Status = x.Status,
                Total = x.Total,
                DocumentDate = x.DocumentDate
            })
            .ToListAsync(cancellationToken);

        var activities = await _db.ActivityLogs.AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(7)
            .Select(x => new DashboardActivityRow
            {
                ActorName = x.ActorName,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityNumber = x.EntityNumber,
                CreatedAt = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var notifications = await _db.Notifications.AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(6)
            .Select(x => new DashboardNotificationRow
            {
                Icon = x.Icon,
                Title = x.Title,
                Message = x.Message,
                Url = x.Url,
                IsRead = x.IsRead
            })
            .ToListAsync(cancellationToken);

        var typeMetrics = await documentsQuery
            .GroupBy(x => x.Type)
            .Select(g => new { Type = g.Key, Count = g.Count(), Total = g.Sum(x => x.Total) })
            .ToListAsync(cancellationToken);

        var model = new DashboardViewModel
        {
            CompanyName = company?.Name ?? "SnapDocs",
            PlanName = subscription?.Plan?.NameAr ?? tenant?.PlanCode ?? "Pro",
            SubscriptionStatus = subscription?.Status.ToString() ?? tenant?.SubscriptionStatus.ToString() ?? "Trial",
            TrialDaysLeft = subscription?.TrialEndsAt is null ? 0 : Math.Max(0, (subscription.TrialEndsAt.Value.Date - today).Days),
            TotalDocuments = totalDocuments,
            MonthDocuments = monthDocuments,
            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newCustomers,
            MonthSales = monthSales,
            OutstandingBalance = Math.Max(0, outstanding),
            OverdueDocuments = overdue,
            CollectionRate = monthSales <= 0 ? 0 : Math.Round((paid / monthSales) * 100, 1),
            DocumentLimit = subscription?.Plan?.DocumentLimit ?? tenant?.MonthlyDocumentLimit ?? 0,
            DocumentUsage = tenant?.MonthlyDocumentCount ?? monthDocuments,
            CustomerLimit = subscription?.Plan?.CustomerLimit ?? 0,
            UserLimit = subscription?.Plan?.UserLimit ?? 1,
            RecentDocuments = recentDocs,
            RecentActivities = activities,
            Notifications = notifications,
            QuickActions = BuildQuickActions(),
            TypeMetrics = typeMetrics.Select(x => new DashboardTypeMetric
            {
                Label = GetTypeLabel(x.Type),
                Icon = GetTypeIcon(x.Type),
                Count = x.Count,
                Total = x.Total
            }).ToList()
        };

        return View(model);
    }

    private Guid GetCompanyId()
    {
        var claim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(claim, out var value) ? value : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirst("TenantId")?.Value;
        return Guid.TryParse(claim, out var value) ? value : Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }

    private static List<DashboardQuickAction> BuildQuickActions() => new()
    {
        new() { Icon = "🧾", Title = "فاتورة بيع", Controller = "Invoices", Style = "primary" },
        new() { Icon = "📋", Title = "كشف حساب", Controller = "CustomerStatements" },
        new() { Icon = "💰", Title = "عرض سعر", Controller = "Quotations" },
        new() { Icon = "📥", Title = "سند قبض", Controller = "ReceiptVouchers" },
        new() { Icon = "📤", Title = "سند صرف", Controller = "PaymentVouchers" },
        new() { Icon = "👥", Title = "عميل جديد", Controller = "Customers" }
    };

    private static string GetTypeLabel(DocumentType type) => type switch
    {
        DocumentType.Invoice => "فواتير",
        DocumentType.Quotation => "عروض أسعار",
        DocumentType.CustomerStatement => "كشوف حساب",
        DocumentType.ReceiptVoucher => "سندات قبض",
        DocumentType.PaymentVoucher => "سندات صرف",
        DocumentType.DeliveryNote => "أذونات تسليم",
        _ => "مستندات"
    };

    private static string GetTypeIcon(DocumentType type) => type switch
    {
        DocumentType.Invoice => "🧾",
        DocumentType.Quotation => "💰",
        DocumentType.CustomerStatement => "📋",
        DocumentType.ReceiptVoucher => "📥",
        DocumentType.PaymentVoucher => "📤",
        DocumentType.DeliveryNote => "📦",
        _ => "📄"
    };
}
