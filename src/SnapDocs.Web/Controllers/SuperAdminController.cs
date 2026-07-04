using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models.SuperAdmin;

namespace SnapDocs.Web.Controllers;

[Authorize]
public class SuperAdminController : Controller
{
    private readonly SnapDocsDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public SuperAdminController(SnapDocsDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var paidRevenue = await _db.SubscriptionPayments
            .Where(x => x.Status == PaymentStatus.Paid)
            .SumAsync(x => x.Amount);

        var tenantsCount = await _db.Tenants.CountAsync();

        var latestTenants = await BuildTenantRows(_db.Tenants.OrderByDescending(x => x.CreatedAtUtc).Take(10));

        var model = new SuperAdminDashboardViewModel
        {
            TenantsCount = tenantsCount,
            CompaniesCount = await _db.Companies.CountAsync(),
            UsersCount = await _db.AppUsers.CountAsync(),
            DocumentsCount = await _db.Documents.CountAsync(),
            ActiveSubscriptions = await _db.TenantSubscriptions.CountAsync(x => x.Status == SubscriptionStatus.Active),
            TrialSubscriptions = await _db.TenantSubscriptions.CountAsync(x => x.Status == SubscriptionStatus.Trial),
            PastDueSubscriptions = await _db.TenantSubscriptions.CountAsync(x => x.Status == SubscriptionStatus.PastDue),
            SuspendedSubscriptions = await _db.TenantSubscriptions.CountAsync(x => x.Status == SubscriptionStatus.Suspended),
            Revenue = paidRevenue,
            OpenBilling = await _db.BillingInvoices.Where(x => x.Status != BillingInvoiceStatus.Paid && x.Status != BillingInvoiceStatus.Cancelled).SumAsync(x => x.Total - x.PaidAmount),
            AverageRevenuePerTenant = tenantsCount == 0 ? 0 : paidRevenue / tenantsCount,
            LatestTenants = latestTenants,
            LatestInvoices = await _db.BillingInvoices.Include(x => x.Plan).OrderByDescending(x => x.IssueDate).Take(10).ToListAsync(),
            LatestActivity = await _db.ActivityLogs.Include(x => x.Company).OrderByDescending(x => x.CreatedAtUtc).Take(10).ToListAsync(),
            RevenueByPlan = await _db.BillingInvoices
                .Include(x => x.Plan)
                .Where(x => x.Status == BillingInvoiceStatus.Paid)
                .GroupBy(x => x.Plan != null ? x.Plan.NameAr : "غير محدد")
                .Select(g => new PlanRevenueRow { PlanName = g.Key, Invoices = g.Count(), Revenue = g.Sum(x => x.PaidAmount) })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Tenants(string? search, SubscriptionStatus? status)
    {
        var query = _db.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search) || x.Slug.Contains(search) || x.PlanCode.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.SubscriptionStatus == status.Value);
        }

        var model = new SuperAdminTenantsViewModel
        {
            Search = search,
            Status = status,
            Tenants = await BuildTenantRows(query.OrderByDescending(x => x.CreatedAtUtc).Take(100))
        };

        return View(model);
    }

    public async Task<IActionResult> TenantDetails(Guid id)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == id);
        if (tenant == null) return NotFound();

        var companyIds = await _db.Companies.Where(x => x.TenantId == id).Select(x => x.Id).ToListAsync();

        var model = new SuperAdminTenantDetailsViewModel
        {
            Tenant = tenant,
            Companies = await _db.Companies.Where(x => x.TenantId == id).OrderBy(x => x.Name).ToListAsync(),
            Users = await _db.AppUsers.Where(x => x.TenantId == id).OrderBy(x => x.FullName).ToListAsync(),
            Subscription = await _db.TenantSubscriptions.Include(x => x.Plan).Where(x => x.TenantId == id).OrderByDescending(x => x.CreatedAtUtc).FirstOrDefaultAsync(),
            BillingInvoices = await _db.BillingInvoices.Include(x => x.Plan).Where(x => x.TenantId == id).OrderByDescending(x => x.IssueDate).Take(20).ToListAsync(),
            Payments = await _db.SubscriptionPayments.Where(x => x.TenantId == id).OrderByDescending(x => x.PaymentDate).Take(20).ToListAsync(),
            LatestDocuments = await _db.Documents.Include(x => x.Customer).Where(x => companyIds.Contains(x.CompanyId)).OrderByDescending(x => x.CreatedAtUtc).Take(20).ToListAsync(),
            LatestActivity = await _db.ActivityLogs.Include(x => x.Company).Where(x => companyIds.Contains(x.CompanyId)).OrderByDescending(x => x.CreatedAtUtc).Take(20).ToListAsync(),
            CustomersCount = await _db.Customers.CountAsync(x => companyIds.Contains(x.CompanyId)),
            ProductsCount = await _db.Products.CountAsync(x => companyIds.Contains(x.CompanyId)),
            DocumentsCount = await _db.Documents.CountAsync(x => companyIds.Contains(x.CompanyId)),
            DocumentsValue = await _db.Documents.Where(x => companyIds.Contains(x.CompanyId)).SumAsync(x => x.Total)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeTenantStatus(Guid id, SubscriptionStatus status)
    {
        var tenant = await _db.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        tenant.SubscriptionStatus = status;
        tenant.UpdatedAtUtc = DateTime.UtcNow;

        var subscription = await _db.TenantSubscriptions.Where(x => x.TenantId == id).OrderByDescending(x => x.CreatedAtUtc).FirstOrDefaultAsync();
        if (subscription != null)
        {
            subscription.Status = status;
            subscription.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تحديث حالة الشركة بنجاح.";
        return RedirectToAction(nameof(TenantDetails), new { id });
    }

    public async Task<IActionResult> Billing(string? search, string? status)
    {
        var query = _db.BillingInvoices.Include(x => x.Plan).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Number.Contains(search) || x.CurrencyCode.Contains(search));
        }

        if (Enum.TryParse<BillingInvoiceStatus>(status, true, out var invoiceStatus))
        {
            query = query.Where(x => x.Status == invoiceStatus);
        }

        var allInvoices = _db.BillingInvoices.AsQueryable();

        var model = new SuperAdminBillingViewModel
        {
            Search = search,
            Status = status,

            IssuedTotal = await allInvoices
                .Where(x => x.Status == BillingInvoiceStatus.Issued)
                .SumAsync(x => x.Total),

            PaidTotal = await allInvoices
                .Where(x => x.Status == BillingInvoiceStatus.Paid)
                .SumAsync(x => x.PaidAmount),

            UnpaidTotal = await allInvoices
                .Where(x => x.Status == BillingInvoiceStatus.Issued)
                .SumAsync(x => x.Total - x.PaidAmount),

            OverdueTotal = await allInvoices
                .Where(x => x.Status == BillingInvoiceStatus.Overdue)
                .SumAsync(x => x.Total - x.PaidAmount),

            InvoiceCount = await allInvoices.CountAsync(),
            IssuedCount = await allInvoices.CountAsync(x => x.Status == BillingInvoiceStatus.Issued),
            PaidCount = await allInvoices.CountAsync(x => x.Status == BillingInvoiceStatus.Paid),
            OverdueCount = await allInvoices.CountAsync(x => x.Status == BillingInvoiceStatus.Overdue),

            Invoices = await query
                .OrderByDescending(x => x.IssueDate)
                .Take(100)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Users(string? search, string? role)
    {
        var query = _db.AppUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.FullName.Contains(search) || x.Email.Contains(search));
        }

        if (Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            query = query.Where(x => x.Role == userRole);
        }

        var model = new SuperAdminUsersViewModel
        {
            Search = search,
            Role = role,
            Users = await query.OrderByDescending(x => x.CreatedAtUtc).Take(100).ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Activity(string? search)
    {
        var query = _db.ActivityLogs.Include(x => x.Company).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.ActorName.Contains(search) || x.Action.Contains(search) || x.EntityName.Contains(search) || (x.EntityNumber != null && x.EntityNumber.Contains(search)));
        }

        var model = new SuperAdminActivityViewModel
        {
            Search = search,
            Logs = await query.OrderByDescending(x => x.CreatedAtUtc).Take(150).ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Health()
    {
        var model = new SuperAdminHealthViewModel
        {
            EnvironmentName = _environment.EnvironmentName,
            ServerUtcNow = DateTime.UtcNow
        };

        try
        {
            model.DatabaseOk = await _db.Database.CanConnectAsync();
            model.DatabaseStatus = model.DatabaseOk ? "Connected" : "Not connected";
            model.Tenants = await _db.Tenants.CountAsync();
            model.Companies = await _db.Companies.CountAsync();
            model.Users = await _db.AppUsers.CountAsync();
            model.Documents = await _db.Documents.CountAsync();
            model.BillingInvoices = await _db.BillingInvoices.CountAsync();
        }
        catch (Exception ex)
        {
            model.DatabaseOk = false;
            model.DatabaseStatus = ex.Message;
        }

        return View(model);
    }

    private async Task<List<TenantAdminRow>> BuildTenantRows(IQueryable<SnapDocs.Domain.Entities.Tenant> query)
    {
        var tenants = await query.ToListAsync();
        var result = new List<TenantAdminRow>();

        foreach (var tenant in tenants)
        {
            var companyIds = await _db.Companies.Where(x => x.TenantId == tenant.Id).Select(x => x.Id).ToListAsync();
            result.Add(new TenantAdminRow
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                PlanCode = tenant.PlanCode,
                Status = tenant.SubscriptionStatus,
                TrialEndsAt = tenant.TrialEndsAt,
                MonthlyDocumentCount = tenant.MonthlyDocumentCount,
                MonthlyDocumentLimit = tenant.MonthlyDocumentLimit,
                Companies = companyIds.Count,
                Users = await _db.AppUsers.CountAsync(x => x.TenantId == tenant.Id),
                Documents = await _db.Documents.CountAsync(x => companyIds.Contains(x.CompanyId)),
                CreatedAtUtc = tenant.CreatedAtUtc
            });
        }

        return result;
    }
}
