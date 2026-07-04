using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Entities.SaaS;

namespace SnapDocs.Web.Models;

public class SuperAdminDashboardViewModel
{
    public int Tenants { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public decimal Revenue { get; set; }
    public List<Tenant> LatestTenants { get; set; } = new();
    public List<BillingInvoice> LatestInvoices { get; set; } = new();
}
