using SnapDocs.Application.DTOs.SaaS;

namespace SnapDocs.Application.Services.SaaS;

public interface ISubscriptionService
{
    Task<TenantUsageDto> GetCurrentUsageAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<BillingDashboardDto> GetBillingDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CanCreateDocumentAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task ChangePlanAsync(Guid tenantId, string planCode, CancellationToken cancellationToken = default);
    Task MarkInvoicePaidAsync(Guid tenantId, Guid billingInvoiceId, CancellationToken cancellationToken = default);
}
