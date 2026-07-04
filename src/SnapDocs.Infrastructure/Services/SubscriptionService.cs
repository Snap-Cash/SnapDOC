using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.DTOs.SaaS;
using SnapDocs.Application.Services.SaaS;
using SnapDocs.Domain.Entities.SaaS;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;

namespace SnapDocs.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly SnapDocsDbContext _db;

    public SubscriptionService(SnapDocsDbContext db) => _db = db;

    public async Task<TenantUsageDto> GetCurrentUsageAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _db.Tenants.FirstAsync(x => x.Id == tenantId, cancellationToken);
        var sub = await _db.TenantSubscriptions.Include(x => x.Plan).FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        var companyIds = await _db.Companies.Where(x => x.TenantId == tenantId).Select(x => x.Id).ToListAsync(cancellationToken);
        var docs = await _db.Documents.CountAsync(x => companyIds.Contains(x.CompanyId), cancellationToken);
        var customers = await _db.Customers.CountAsync(x => companyIds.Contains(x.CompanyId), cancellationToken);
        var users = await _db.AppUsers.CountAsync(x => x.TenantId == tenantId, cancellationToken);
        var plan = sub?.Plan;

        return new TenantUsageDto
        {
            TenantName = tenant.Name,
            PlanCode = tenant.PlanCode,
            Status = tenant.SubscriptionStatus.ToString(),
            DocumentsUsed = docs,
            DocumentLimit = plan?.DocumentLimit ?? tenant.MonthlyDocumentLimit,
            UsersUsed = users,
            UserLimit = plan?.UserLimit ?? 1,
            CustomersUsed = customers,
            CustomerLimit = plan?.CustomerLimit ?? 25,
            TrialEndsAt = sub?.TrialEndsAt ?? tenant.TrialEndsAt
        };
    }

    public async Task<BillingDashboardDto> GetBillingDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _db.Tenants.FirstAsync(x => x.Id == tenantId, cancellationToken);
        var plans = await _db.SubscriptionPlans
            .Where(x => x.IsActive)
            .OrderBy(x => x.MonthlyPrice)
            .Select(x => new PlanCardDto
            {
                Id = x.Id,
                Code = x.Code,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                MonthlyPrice = x.MonthlyPrice,
                DocumentLimit = x.DocumentLimit,
                UserLimit = x.UserLimit,
                CustomerLimit = x.CustomerLimit,
                HasWatermark = x.HasWatermark,
                CanUseWhatsApp = x.CanUseWhatsApp,
                CanUseCustomTemplates = x.CanUseCustomTemplates,
                CanUseApi = x.CanUseApi,
                IsCurrent = x.Code == tenant.PlanCode
            })
            .ToListAsync(cancellationToken);

        var invoices = await _db.BillingInvoices
            .Include(x => x.Plan)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.IssueDate)
            .Take(20)
            .Select(x => new BillingInvoiceDto
            {
                Id = x.Id,
                Number = x.Number,
                PlanName = x.Plan != null ? x.Plan.NameAr : x.SubscriptionPlanId.ToString(),
                Status = x.Status.ToString(),
                IssueDate = x.IssueDate,
                DueDate = x.DueDate,
                Total = x.Total,
                PaidAmount = x.PaidAmount,
                CurrencyCode = x.CurrencyCode
            })
            .ToListAsync(cancellationToken);

        return new BillingDashboardDto
        {
            Usage = await GetCurrentUsageAsync(tenantId, cancellationToken),
            Plans = plans,
            Invoices = invoices,
            CurrencyCode = "EGP",
            CurrentBalance = invoices.Where(x => x.Status != BillingInvoiceStatus.Paid.ToString() && x.Status != BillingInvoiceStatus.Cancelled.ToString()).Sum(x => x.Total - x.PaidAmount)
        };
    }

    public async Task<bool> CanCreateDocumentAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var usage = await GetCurrentUsageAsync(tenantId, cancellationToken);
        return usage.DocumentLimit <= 0 || usage.DocumentsUsed < usage.DocumentLimit;
    }

    public async Task ChangePlanAsync(Guid tenantId, string planCode, CancellationToken cancellationToken = default)
    {
        var tenant = await _db.Tenants.FirstAsync(x => x.Id == tenantId, cancellationToken);
        var plan = await _db.SubscriptionPlans.FirstAsync(x => x.Code == planCode && x.IsActive, cancellationToken);
        var sub = await _db.TenantSubscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        tenant.PlanCode = plan.Code;
        tenant.MonthlyDocumentLimit = plan.DocumentLimit;
        tenant.SubscriptionStatus = plan.MonthlyPrice == 0 ? SubscriptionStatus.Active : SubscriptionStatus.PastDue;
        tenant.UpdatedAtUtc = DateTime.UtcNow;

        if (sub == null)
        {
            sub = new TenantSubscription { TenantId = tenantId, PlanId = plan.Id, StartsAt = DateTime.UtcNow };
            _db.TenantSubscriptions.Add(sub);
        }

        sub.PlanId = plan.Id;
        sub.Status = tenant.SubscriptionStatus;
        sub.EndsAt = DateTime.UtcNow.AddMonths(1);
        sub.AutoRenew = false;
        sub.UpdatedAtUtc = DateTime.UtcNow;

        if (plan.MonthlyPrice > 0)
        {
            var invoiceNo = $"BILL-{DateTime.UtcNow:yyyyMM}-{await _db.BillingInvoices.CountAsync(x => x.TenantId == tenantId, cancellationToken) + 1:0000}";
            var tax = Math.Round(plan.MonthlyPrice * 0.14m, 2);
            _db.BillingInvoices.Add(new BillingInvoice
            {
                TenantId = tenantId,
                SubscriptionPlanId = plan.Id,
                Number = invoiceNo,
                Status = BillingInvoiceStatus.Issued,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                SubTotal = plan.MonthlyPrice,
                Tax = tax,
                Total = plan.MonthlyPrice + tax,
                CurrencyCode = "EGP",
                Notes = $"اشتراك باقة {plan.NameAr} لمدة شهر"
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkInvoicePaidAsync(Guid tenantId, Guid billingInvoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _db.BillingInvoices.FirstAsync(x => x.Id == billingInvoiceId && x.TenantId == tenantId, cancellationToken);
        invoice.Status = BillingInvoiceStatus.Paid;
        invoice.PaidAmount = invoice.Total;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        _db.SubscriptionPayments.Add(new SubscriptionPayment
        {
            TenantId = tenantId,
            BillingInvoiceId = invoice.Id,
            Amount = invoice.Total,
            CurrencyCode = invoice.CurrencyCode,
            Provider = "Manual",
            ReferenceNumber = $"MAN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow,
            Notes = "تم تسجيل الدفع يدويًا من شاشة الاشتراك"
        });

        var tenant = await _db.Tenants.FirstAsync(x => x.Id == tenantId, cancellationToken);
        var sub = await _db.TenantSubscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        tenant.SubscriptionStatus = SubscriptionStatus.Active;
        if (sub != null)
        {
            sub.Status = SubscriptionStatus.Active;
            sub.EndsAt = DateTime.UtcNow.AddMonths(1);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
