using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapDocs.Application.Services.SaaS;

namespace SnapDocs.Web.Controllers;

[Authorize]
public class SubscriptionController : Controller
{
    private readonly ISubscriptionService _subscriptionService;
    public SubscriptionController(ISubscriptionService subscriptionService) => _subscriptionService = subscriptionService;

    public async Task<IActionResult> Index()
    {
        var tenantId = GetTenantId();
        var model = await _subscriptionService.GetBillingDashboardAsync(tenantId);
        return View(model);
    }

    public async Task<IActionResult> Pricing()
    {
        var tenantId = GetTenantId();
        var model = await _subscriptionService.GetBillingDashboardAsync(tenantId);
        return View(model);
    }

    public async Task<IActionResult> Billing()
    {
        var tenantId = GetTenantId();
        var model = await _subscriptionService.GetBillingDashboardAsync(tenantId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePlan(string planCode)
    {
        var tenantId = GetTenantId();
        await _subscriptionService.ChangePlanAsync(tenantId, planCode);
        TempData["Success"] = "تم تحديث الباقة وإنشاء فاتورة اشتراك إذا كانت الباقة مدفوعة.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(Guid invoiceId)
    {
        var tenantId = GetTenantId();
        await _subscriptionService.MarkInvoicePaidAsync(tenantId, invoiceId);
        TempData["Success"] = "تم تسجيل الدفع وتفعيل الاشتراك.";
        return RedirectToAction(nameof(Billing));
    }

    private Guid GetTenantId() => Guid.Parse(User.FindFirst("TenantId")!.Value);
}
