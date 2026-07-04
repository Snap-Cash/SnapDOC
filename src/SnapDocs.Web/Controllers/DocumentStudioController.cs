using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class DocumentStudioController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;

    public DocumentStudioController(SnapDocsDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Show(Guid id)
    {
        var document = await _db.Documents
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId);

        if (document is null) return NotFound();

        var verifyUrl = Url.Action("Verify", "Print", new { code = document.VerifyCode }, Request.Scheme) ?? string.Empty;
        var model = DocumentStudioCatalog.Create(document, verifyUrl);
        return View(model);
    }
}
