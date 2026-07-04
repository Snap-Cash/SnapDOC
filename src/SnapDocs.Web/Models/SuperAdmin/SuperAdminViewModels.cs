using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Entities.Identity;
using SnapDocs.Domain.Entities.SaaS;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models.SuperAdmin;

public class SuperAdminDashboardViewModel
{
    public int TenantsCount { get; set; }
    public int CompaniesCount { get; set; }
    public int UsersCount { get; set; }
    public int DocumentsCount { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public int PastDueSubscriptions { get; set; }
    public int SuspendedSubscriptions { get; set; }
    public decimal Revenue { get; set; }
    public decimal OpenBilling { get; set; }
    public decimal AverageRevenuePerTenant { get; set; }
    public List<TenantAdminRow> LatestTenants { get; set; } = new();
    public List<BillingInvoice> LatestInvoices { get; set; } = new();
    public List<ActivityLog> LatestActivity { get; set; } = new();
    public List<PlanRevenueRow> RevenueByPlan { get; set; } = new();
}

public class TenantAdminRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public DateTime TrialEndsAt { get; set; }
    public int MonthlyDocumentCount { get; set; }
    public int MonthlyDocumentLimit { get; set; }
    public int Companies { get; set; }
    public int Users { get; set; }
    public int Documents { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class PlanRevenueRow
{
    public string PlanName { get; set; } = string.Empty;
    public int Invoices { get; set; }
    public decimal Revenue { get; set; }
}

public class SuperAdminTenantsViewModel
{
    public string? Search { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public List<TenantAdminRow> Tenants { get; set; } = new();
}

public class SuperAdminTenantDetailsViewModel
{
    public Tenant Tenant { get; set; } = default!;
    public List<Company> Companies { get; set; } = new();
    public List<AppUser> Users { get; set; } = new();
    public TenantSubscription? Subscription { get; set; }
    public List<BillingInvoice> BillingInvoices { get; set; } = new();
    public List<SubscriptionPayment> Payments { get; set; } = new();
    public List<Document> LatestDocuments { get; set; } = new();
    public List<ActivityLog> LatestActivity { get; set; } = new();
    public int CustomersCount { get; set; }
    public int ProductsCount { get; set; }
    public int DocumentsCount { get; set; }
    public decimal DocumentsValue { get; set; }
}

public class SuperAdminBillingViewModel
{
    public string? Search { get; set; }
    public string? Status { get; set; }

    public decimal IssuedTotal { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal UnpaidTotal { get; set; }
    public decimal OverdueTotal { get; set; }
    public decimal OutstandingTotal => UnpaidTotal + OverdueTotal;

    public int InvoiceCount { get; set; }
    public int IssuedCount { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }

    public List<BillingInvoice> Invoices { get; set; } = new();
}

public class SuperAdminUsersViewModel
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public List<AppUser> Users { get; set; } = new();
}

public class SuperAdminActivityViewModel
{
    public string? Search { get; set; }
    public List<ActivityLog> Logs { get; set; } = new();
}

public class SuperAdminHealthViewModel
{
    public bool DatabaseOk { get; set; }
    public string DatabaseStatus { get; set; } = string.Empty;
    public int Tenants { get; set; }
    public int Companies { get; set; }
    public int Users { get; set; }
    public int Documents { get; set; }
    public int BillingInvoices { get; set; }
    public DateTime ServerUtcNow { get; set; } = DateTime.UtcNow;
    public string EnvironmentName { get; set; } = string.Empty;
}
