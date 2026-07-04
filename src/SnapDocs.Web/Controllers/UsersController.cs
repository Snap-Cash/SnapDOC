using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;

namespace SnapDocs.Web.Controllers;

public class UsersController : Controller
{
    private readonly SnapDocsDbContext _db;
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public UsersController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var users = await _db.CompanyUsers.Where(x => x.CompanyId == DemoCompanyId).OrderBy(x => x.FullName).ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create() => View(new CompanyUser { CompanyId = DemoCompanyId, Role = UserRole.Sales, IsActive = true, CanCreateDocuments = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyUser model)
    {
        model.CompanyId = DemoCompanyId;
        if (!ModelState.IsValid) return View(model);
        _db.CompanyUsers.Add(model);
        _db.ActivityLogs.Add(new ActivityLog { CompanyId = DemoCompanyId, ActorName = "Admin", Action = "إضافة مستخدم", EntityName = "User", EntityNumber = model.Email });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
