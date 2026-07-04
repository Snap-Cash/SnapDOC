using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Services.Print;

namespace SnapDocs.Web.Controllers;

public class PrintController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly IDocumentPrintService _printService;
    private readonly SnapDocsDbContext _db;

    public PrintController(IDocumentPrintService printService, SnapDocsDbContext db)
    {
        _printService = printService;
        _db = db;
    }

    public async Task<IActionResult> Document(Guid id, bool auto = false)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var model = await _printService.BuildAsync(id, DemoCompanyId, baseUrl);
        if (model is null) return NotFound();

        ViewBag.AutoPrint = auto;
        return View(model);
    }

    public async Task<IActionResult> Preview(Guid id)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var model = await _printService.BuildAsync(id, DemoCompanyId, baseUrl);
        if (model is null) return NotFound();
        return View(model);
    }

    [HttpGet("verify/{code}")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Verify(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return NotFound();

        var document = await _db.Documents
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.VerifyCode == code.Trim());

        if (document is null) return NotFound();
        return View(document);
    }

    public async Task<IActionResult> Templates(DocumentType? type)
    {
        var query = _db.DocumentTemplateSettings.Where(x => x.CompanyId == DemoCompanyId).AsQueryable();
        if (type.HasValue) query = query.Where(x => x.DocumentType == type.Value);
        var settings = await query.OrderBy(x => x.DocumentType).ToListAsync();
        ViewBag.DocumentType = type;
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTemplate(Guid id, string templateName, string accentColor, bool showQrCode, bool showWatermark, string? footerText)
    {
        var setting = await _db.DocumentTemplateSettings.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId);
        if (setting is null) return NotFound();

        setting.TemplateName = string.IsNullOrWhiteSpace(templateName) ? "Corporate" : templateName.Trim();
        setting.AccentColor = string.IsNullOrWhiteSpace(accentColor) ? "#2563eb" : accentColor.Trim();
        setting.ShowQrCode = showQrCode;
        setting.ShowWatermark = showWatermark;
        setting.FooterText = footerText;
        await _db.SaveChangesAsync();

        TempData["Success"] = "تم تحديث قالب الطباعة بنجاح.";
        return RedirectToAction(nameof(Templates), new { type = setting.DocumentType });
    }
}
