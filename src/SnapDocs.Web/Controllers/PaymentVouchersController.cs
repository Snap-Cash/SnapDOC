using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.DTOs;
using SnapDocs.Application.Services;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class PaymentVouchersController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;
    private readonly IDocumentService _documentService;
    private readonly IDocumentWorkflowService _workflowService;

    public PaymentVouchersController(SnapDocsDbContext db, IDocumentService documentService, IDocumentWorkflowService workflowService)
    {
        _db = db;
        _documentService = documentService;
        _workflowService = workflowService;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _db.Documents
            .Include(x => x.Customer)
            .Where(x => x.CompanyId == DemoCompanyId && x.Type == DocumentType.PaymentVoucher)
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
        return View(documents);
    }

    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View(new PaymentVoucherCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentVoucherCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View(model);
        }

        var dto = new CreateDocumentDto { CompanyId = DemoCompanyId, CustomerId = model.CustomerId, Type = DocumentType.PaymentVoucher, DocumentDate = model.VoucherDate, DueDate = null, Discount = 0, TaxRate = 0, Notes = $"طريقة الدفع: {model.PaymentMethod} | المرجع: {model.ReferenceNumber} | {model.Notes}", Items = new List<DocumentItemInputDto> { new() { Description = "سند صرف", Quantity = 1, UnitPrice = model.Amount, Discount = 0 } } };
        var document = await _documentService.CreateAsync(dto);
        var cashAccount = model.CashAccountId.HasValue
            ? await _db.CashAccounts.FirstOrDefaultAsync(x => x.Id == model.CashAccountId.Value && x.CompanyId == DemoCompanyId && x.IsActive)
            : await _db.CashAccounts.FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.IsDefault && x.IsActive);

        if (cashAccount is not null)
        {
            _db.CashTransactions.Add(new CashTransaction
            {
                CompanyId = DemoCompanyId,
                CashAccountId = cashAccount.Id,
                DocumentId = document.Id,
                CustomerId = document.CustomerId,
                Type = CashTransactionType.Payment,
                TransactionDate = model.VoucherDate,
                Debit = 0,
                Credit = model.Amount,
                PaymentMethod = model.PaymentMethod,
                ReferenceNumber = model.ReferenceNumber,
                Description = document.Number
            });
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = $"تم إنشاء سند الصرف {document.Number} بنجاح";
        return RedirectToAction(nameof(Details), new { id = document.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var document = await _db.Documents
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.PaymentVoucher);

        if (document is null) return NotFound();
        return View(document);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, DocumentStatus status)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.PaymentVoucher);
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

        var cashAccounts = await _db.CashAccounts
            .Where(x => x.CompanyId == DemoCompanyId && x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync();
        ViewBag.CashAccounts = new SelectList(cashAccounts, "Id", "Name");
    }
}
