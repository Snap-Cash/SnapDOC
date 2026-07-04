using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Infrastructure.Persistence;

namespace SnapDocs.Web.Controllers;

public class SaaSController : Controller
{
    private readonly SnapDocsDbContext _db;
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public SaaSController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var company = await _db.Companies.Include(x => x.Tenant).FirstAsync(x => x.Id == DemoCompanyId);
        return View(company);
    }
}
