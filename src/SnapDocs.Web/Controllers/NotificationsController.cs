using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Infrastructure.Persistence;

namespace SnapDocs.Web.Controllers;

public class NotificationsController : Controller
{
    private readonly SnapDocsDbContext _db;
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public NotificationsController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var notifications = await _db.Notifications.Where(x => x.CompanyId == DemoCompanyId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync();
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var items = await _db.Notifications.Where(x => x.CompanyId == DemoCompanyId && !x.IsRead).ToListAsync();
        foreach (var item in items) item.IsRead = true;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
