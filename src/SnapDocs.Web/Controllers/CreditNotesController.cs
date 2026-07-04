using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.DTOs;
using SnapDocs.Application.Services;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class CreditNotesController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;
    private readonly IDocumentService _documentService;
    private readonly IDocumentWorkflowService _workflowService;

    public CreditNotesController(SnapDocsDbContext db, IDocumentService documentService, IDocumentWorkflowService workflowService)
    {
        _db = db;
        _documentService = documentService;
        _workflowService = workflowService;
    }

    public async Task<IActionResult> Index(string? q, DocumentStatus? status)
    {
        var query = _db.Documents.Include(x => x.Customer)
            .Where(x => x.CompanyId == DemoCompanyId && x.Type == DocumentType.CreditNote);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Number.Contains(q) || x.VerifyCode.Contains(q) || (x.Customer != null && x.Customer.Name.Contains(q)));
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        ViewBag.Query = q;
        ViewBag.Status = status;
        var documents = await query.OrderByDescending(x => x.DocumentDate).ThenByDescending(x => x.CreatedAtUtc).ToListAsync();
        return View(documents);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View(new CreditNoteCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreditNoteCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View(model);
        }

        var dto = new CreateDocumentDto
        {
            CompanyId = DemoCompanyId,
            CustomerId = model.CustomerId,
            Type = DocumentType.CreditNote,
            DocumentDate = model.DocumentDate,
            Notes = $"السبب: {model.Reason} | المرجع: {model.ReferenceNumber} | {model.Notes}",
            Items = new List<DocumentItemInputDto>
            {
                new() { Description = model.Reason, Quantity = 1, UnitPrice = model.Amount, Discount = 0 }
            }
        };

        var document = await _documentService.CreateAsync(dto);
        TempData["Success"] = $"تم إنشاء الإشعار الدائن {document.Number} بنجاح";
        return RedirectToAction(nameof(Details), new { id = document.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var document = await _db.Documents.Include(x => x.Customer).Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.CreditNote);
        if (document is null) return NotFound();
        return View(document);
    }

    public IActionResult Print(Guid id)
    {
        return RedirectToAction("Document", "Print", new { id, auto = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, DocumentStatus status)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.CreditNote);
        if (document is null) return NotFound();
        _workflowService.Apply(document, status);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"تم تحديث حالة الإشعار إلى {status}";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLookups()
    {
        var customers = await _db.Customers.Where(x => x.CompanyId == DemoCompanyId && x.IsActive).OrderBy(x => x.Name).ToListAsync();
        ViewBag.Customers = new SelectList(customers, "Id", "Name");
    }
}
