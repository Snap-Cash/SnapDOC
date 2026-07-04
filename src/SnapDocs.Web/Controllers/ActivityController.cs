using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Infrastructure.Persistence;

namespace SnapDocs.Web.Controllers;

public class ActivityController : Controller
{
    private readonly SnapDocsDbContext _db;
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public ActivityController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var logs = await _db.ActivityLogs.Where(x => x.CompanyId == DemoCompanyId).OrderByDescending(x => x.CreatedAtUtc).Take(100).ToListAsync();
        return View(logs);
    }
}
