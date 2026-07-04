using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.DTOs;
using SnapDocs.Application.Services;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class DeliveryNotesController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;
    private readonly IDocumentService _documentService;
    private readonly IDocumentWorkflowService _workflowService;

    public DeliveryNotesController(SnapDocsDbContext db, IDocumentService documentService, IDocumentWorkflowService workflowService)
    {
        _db = db;
        _documentService = documentService;
        _workflowService = workflowService;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _db.Documents
            .Include(x => x.Customer)
            .Where(x => x.CompanyId == DemoCompanyId && x.Type == DocumentType.DeliveryNote)
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
        return View(documents);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View(new DeliveryNoteCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeliveryNoteCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View(model);
        }

        var dto = new CreateDocumentDto { CompanyId = DemoCompanyId, CustomerId = model.CustomerId, Type = DocumentType.DeliveryNote, DocumentDate = model.DeliveryDate, DueDate = null, Discount = 0, TaxRate = 0, Notes = $"عنوان التسليم: {model.DeliveryAddress} | المستلم: {model.ReceiverName} | {model.Notes}", Items = model.Items.Select(x => new DocumentItemInputDto { Description = x.Description, Quantity = x.Quantity, UnitPrice = 0, Discount = 0 }).ToList() };
        var document = await _documentService.CreateAsync(dto);
        TempData["Success"] = $"تم إنشاء إذن التسليم {document.Number} بنجاح";
        return RedirectToAction(nameof(Details), new { id = document.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var document = await _db.Documents
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.DeliveryNote);

        if (document is null) return NotFound();
        return View(document);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, DocumentStatus status)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.DeliveryNote);
        if (document is null) return NotFound();

        _workflowService.Apply(document, status);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"تم تحديث حالة المستند إلى {status}";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLookups()
    {
        var customers = await _db.Customers
            .Where(x => x.CompanyId == DemoCompanyId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
        ViewBag.Customers = new SelectList(customers, "Id", "Name");
    }
}
