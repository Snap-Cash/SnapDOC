using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.DTOs;
using SnapDocs.Application.Services;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class InvoicesController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;
    private readonly IDocumentService _documentService;
    private readonly IDocumentWorkflowService _workflowService;

    public InvoicesController(SnapDocsDbContext db, IDocumentService documentService, IDocumentWorkflowService workflowService)
    {
        _db = db;
        _documentService = documentService;
        _workflowService = workflowService;
    }

    public async Task<IActionResult> Index(string? q, DocumentStatus? status)
    {
        var query = _db.Documents
            .Include(x => x.Customer)
            .Where(x => x.CompanyId == DemoCompanyId && x.Type == DocumentType.Invoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.Number.Contains(q) || x.VerifyCode.Contains(q) || (x.Customer != null && x.Customer.Name.Contains(q)));
        }

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var documents = await query
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Status = status;
        return View(documents);
    }

    public async Task<IActionResult> Create(Guid? customerId)
    {
        await LoadLookups();
        return View(new InvoiceCreateViewModel { CustomerId = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceCreateViewModel model)
    {
        model.Items = model.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.Description) || x.UnitPrice > 0)
            .ToList();

        if (!model.Items.Any())
            ModelState.AddModelError("Items", "يجب إضافة بند واحد على الأقل داخل الفاتورة.");

        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View(model);
        }

        var dto = new CreateDocumentDto
        {
            CompanyId = DemoCompanyId,
            CustomerId = model.CustomerId,
            Type = DocumentType.Invoice,
            DocumentDate = model.DocumentDate,
            DueDate = model.DueDate,
            Discount = model.Discount,
            TaxRate = model.TaxRate,
            Notes = model.Notes,
            Items = model.Items.Select(x => new DocumentItemInputDto
            {
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Discount = x.Discount
            }).ToList()
        };

        var document = await _documentService.CreateAsync(dto);
        TempData["Success"] = $"تم إنشاء فاتورة البيع {document.Number} بنجاح";
        return RedirectToAction(nameof(Details), new { id = document.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var document = await GetInvoice(id);
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
        var document = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.Invoice);
        if (document is null) return NotFound();

        _workflowService.Apply(document, status);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"تم تحديث حالة الفاتورة إلى {status}";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(Guid id)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.Invoice);
        if (document is null) return NotFound();

        document.PaidAmount = document.Total;
        _workflowService.Apply(document, DocumentStatus.Paid);
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تسجيل الفاتورة كمدفوعة بالكامل.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Duplicate(Guid id)
    {
        var document = await GetInvoice(id);
        if (document is null) return NotFound();

        await LoadLookups();
        var model = new InvoiceCreateViewModel
        {
            CustomerId = document.CustomerId,
            DocumentDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(7),
            Discount = document.Discount,
            TaxRate = document.TaxRate,
            Notes = document.Notes,
            Items = document.Items.Select(x => new InvoiceItemViewModel
            {
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Discount = x.Discount
            }).ToList()
        };

        return View(nameof(Create), model);
    }

    private Task<SnapDocs.Domain.Entities.Document?> GetInvoice(Guid id)
    {
        return _db.Documents
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.Invoice);
    }

    private async Task LoadLookups()
    {
        var customers = await _db.Customers
            .Where(x => x.CompanyId == DemoCompanyId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        var products = await _db.Products
            .Where(x => x.CompanyId == DemoCompanyId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                code = x.Code,
                unitName = x.UnitName,
                salePrice = x.SalePrice,
                taxRate = x.TaxRate,
                description = string.IsNullOrWhiteSpace(x.Description) ? x.Name : x.Description
            })
            .ToListAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name");
        ViewBag.Products = new SelectList(products, "id", "name");
        ViewBag.ProductCatalog = products;
    }
}
