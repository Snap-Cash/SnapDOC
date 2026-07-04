using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Infrastructure.Persistence;

namespace SnapDocs.Web.Controllers;

public class TemplatesController : Controller
{
    private readonly SnapDocsDbContext _db;
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public TemplatesController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var templates = await _db.DocumentTemplateSettings.Where(x => x.CompanyId == DemoCompanyId).OrderBy(x => x.DocumentType).ToListAsync();
        return View(templates);
    }
}
