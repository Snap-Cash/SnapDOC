using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;
using System.Globalization;

namespace SnapDocs.Web.Controllers;

public class CustomerStatementsController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;

    public CustomerStatementsController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var documents = await _db.Documents
            .Include(x => x.Customer)
            .Where(x => x.CompanyId == DemoCompanyId && x.Type == DocumentType.CustomerStatement)
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
        return View(documents);
    }

    public async Task<IActionResult> Create(Guid? customerId = null)
    {
        await LoadLookups();
        return View(new CustomerStatementFilterViewModel { CustomerId = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerStatementFilterViewModel model)
    {
        if (!model.CustomerId.HasValue)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "اختر العميل أولًا.");
        }

        if (model.FromDate > model.ToDate)
        {
            ModelState.AddModelError(nameof(model.ToDate), "تاريخ النهاية يجب أن يكون بعد تاريخ البداية.");
        }

        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View(model);
        }

        return RedirectToAction(nameof(Preview), new
        {
            customerId = model.CustomerId,
            fromDate = model.FromDate.ToString("yyyy-MM-dd"),
            toDate = model.ToDate.ToString("yyyy-MM-dd"),
            includeDrafts = model.IncludeDrafts,
            notes = model.Notes
        });
    }

    public async Task<IActionResult> Preview(Guid customerId, DateTime fromDate, DateTime toDate, bool includeDrafts = false, string? notes = null)
    {
        var model = await BuildStatementAsync(customerId, fromDate, toDate, includeDrafts, notes);
        if (model is null) return NotFound();
        return View("Details", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSnapshot(Guid customerId, DateTime fromDate, DateTime toDate, bool includeDrafts = false, string? notes = null)
    {
        var statement = await BuildStatementAsync(customerId, fromDate, toDate, includeDrafts, notes);
        if (statement is null) return NotFound();

        var number = await GenerateStatementNumberAsync();
        var snapshot = new Document
        {
            CompanyId = DemoCompanyId,
            CustomerId = customerId,
            Type = DocumentType.CustomerStatement,
            Status = DocumentStatus.Draft,
            Number = number,
            DocumentDate = DateTime.Today,
            SubTotal = statement.TotalDebit,
            Discount = statement.TotalCredit,
            TaxRate = 0,
            Tax = 0,
            Total = statement.ClosingBalance,
            Notes = $"From={fromDate:yyyy-MM-dd};To={toDate:yyyy-MM-dd};Opening={statement.OpeningBalance.ToString(CultureInfo.InvariantCulture)};IncludeDrafts={includeDrafts};Notes={notes ?? string.Empty}",
            Items = statement.Lines.Select(x => new DocumentItem
            {
                Description = EncodeLine(x),
                Quantity = 1,
                UnitPrice = x.Debit,
                Discount = x.Credit,
                LineTotal = x.Debit - x.Credit
            }).ToList()
        };

        _db.Documents.Add(snapshot);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"تم حفظ كشف الحساب {snapshot.Number} بنجاح";
        return RedirectToAction(nameof(Details), new { id = snapshot.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var document = await _db.Documents
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.CustomerStatement);

        if (document is null || document.Customer is null) return NotFound();

        var meta = ParseNotes(document.Notes);
        var fromDate = meta.FromDate ?? document.DocumentDate;
        var toDate = meta.ToDate ?? document.DocumentDate;
        var opening = meta.OpeningBalance;
        var lines = document.Items
            .OrderBy(x => x.Id)
            .Select(x => DecodeLine(x.Description, x.UnitPrice, x.Discount))
            .ToList();

        var running = opening;
        foreach (var line in lines)
        {
            running += line.Debit - line.Credit;
            line.Balance = running;
        }

        var model = new CustomerStatementDetailsViewModel
        {
            SnapshotId = document.Id,
            StatementNumber = document.Number,
            VerifyCode = document.VerifyCode,
            Status = document.Status,
            Customer = document.Customer,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = opening,
            TotalDebit = lines.Sum(x => x.Debit),
            TotalCredit = lines.Sum(x => x.Credit),
            ClosingBalance = running,
            Notes = meta.Notes,
            IsSnapshot = true,
            Lines = lines
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, DocumentStatus status)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DemoCompanyId && x.Type == DocumentType.CustomerStatement);
        if (document is null) return NotFound();

        document.Status = status;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"تم تحديث حالة كشف الحساب إلى {status}";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<CustomerStatementDetailsViewModel?> BuildStatementAsync(Guid customerId, DateTime fromDate, DateTime toDate, bool includeDrafts, string? notes)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(x => x.Id == customerId && x.CompanyId == DemoCompanyId);
        if (customer is null) return null;

        var allDocuments = await _db.Documents
            .Where(x => x.CompanyId == DemoCompanyId && x.CustomerId == customerId && x.Type != DocumentType.CustomerStatement)
            .Where(x => includeDrafts || x.Status != DocumentStatus.Draft)
            .Where(x => x.Status != DocumentStatus.Cancelled)
            .OrderBy(x => x.DocumentDate)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync();

        var openingBalance = customer.OpeningBalance;
        foreach (var doc in allDocuments.Where(x => x.DocumentDate.Date < fromDate.Date))
        {
            var movement = GetMovement(doc);
            openingBalance += movement.Debit - movement.Credit;
        }

        var running = openingBalance;
        var lines = new List<CustomerStatementLineViewModel>();
        foreach (var doc in allDocuments.Where(x => x.DocumentDate.Date >= fromDate.Date && x.DocumentDate.Date <= toDate.Date))
        {
            var movement = GetMovement(doc);
            if (movement.Debit == 0 && movement.Credit == 0) continue;
            running += movement.Debit - movement.Credit;

            lines.Add(new CustomerStatementLineViewModel
            {
                Date = doc.DocumentDate,
                TypeName = GetTypeName(doc.Type),
                DocumentNumber = doc.Number,
                Description = movement.Description,
                Debit = movement.Debit,
                Credit = movement.Credit,
                Balance = running,
                Status = doc.Status,
                SourceDocumentId = doc.Id
            });
        }

        return new CustomerStatementDetailsViewModel
        {
            StatementNumber = "معاينة",
            Customer = customer,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            TotalDebit = lines.Sum(x => x.Debit),
            TotalCredit = lines.Sum(x => x.Credit),
            ClosingBalance = running,
            Notes = notes,
            IsSnapshot = false,
            Lines = lines
        };
    }

    private static (decimal Debit, decimal Credit, string Description) GetMovement(Document doc)
    {
        return doc.Type switch
        {
            DocumentType.Invoice => (doc.Total, doc.PaidAmount, "فاتورة بيع"),
            DocumentType.DebitNote => (doc.Total, 0, "إشعار مدين"),
            DocumentType.ReceiptVoucher => (0, doc.Total, "سند قبض"),
            DocumentType.PaymentVoucher => (0, doc.Total, "سند صرف / رد مبلغ"),
            DocumentType.CreditNote => (0, doc.Total, "إشعار دائن"),
            _ => (0, 0, GetTypeName(doc.Type))
        };
    }

    private async Task<string> GenerateStatementNumberAsync()
    {
        var year = DateTime.Today.Year;
        var count = await _db.Documents.CountAsync(x => x.CompanyId == DemoCompanyId && x.Type == DocumentType.CustomerStatement && x.DocumentDate.Year == year);
        return $"STM-{year}-{count + 1:00000}";
    }

    private static string GetTypeName(DocumentType type) => type switch
    {
        DocumentType.Invoice => "فاتورة",
        DocumentType.Quotation => "عرض سعر",
        DocumentType.ReceiptVoucher => "سند قبض",
        DocumentType.PaymentVoucher => "سند صرف",
        DocumentType.DeliveryNote => "إذن تسليم",
        DocumentType.CustomerStatement => "كشف حساب",
        DocumentType.CreditNote => "إشعار دائن",
        DocumentType.DebitNote => "إشعار مدين",
        _ => type.ToString()
    };

    private static string EncodeLine(CustomerStatementLineViewModel line)
    {
        static string Clean(string value) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value ?? string.Empty));
        return string.Join("|", line.Date.ToString("yyyy-MM-dd"), Clean(line.TypeName), Clean(line.DocumentNumber), Clean(line.Description), line.Debit.ToString(CultureInfo.InvariantCulture), line.Credit.ToString(CultureInfo.InvariantCulture), line.Status.ToString());
    }

    private static CustomerStatementLineViewModel DecodeLine(string encoded, decimal fallbackDebit, decimal fallbackCredit)
    {
        static string Read(string value)
        {
            try { return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value)); }
            catch { return value; }
        }

        var parts = encoded.Split('|');
        if (parts.Length >= 7)
        {
            Enum.TryParse<DocumentStatus>(parts[6], out var status);
            return new CustomerStatementLineViewModel
            {
                Date = DateTime.TryParse(parts[0], out var date) ? date : DateTime.Today,
                TypeName = Read(parts[1]),
                DocumentNumber = Read(parts[2]),
                Description = Read(parts[3]),
                Debit = decimal.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var debit) ? debit : 0,
                Credit = decimal.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var credit) ? credit : 0,
                Status = status
            };
        }

        return new CustomerStatementLineViewModel
        {
            Date = DateTime.Today,
            TypeName = "حركة",
            DocumentNumber = "-",
            Description = encoded,
            Debit = fallbackDebit,
            Credit = fallbackCredit,
            Status = DocumentStatus.Draft
        };
    }

    private static (DateTime? FromDate, DateTime? ToDate, decimal OpeningBalance, string? Notes) ParseNotes(string? notes)
    {
        DateTime? from = null;
        DateTime? to = null;
        decimal opening = 0;
        string? cleanNotes = null;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            foreach (var part in notes.Split(';'))
            {
                var pair = part.Split('=', 2);
                if (pair.Length != 2) continue;
                if (pair[0] == "From" && DateTime.TryParse(pair[1], out var f)) from = f;
                if (pair[0] == "To" && DateTime.TryParse(pair[1], out var t)) to = t;
                if (pair[0] == "Opening" && decimal.TryParse(pair[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var o)) opening = o;
                if (pair[0] == "Notes") cleanNotes = pair[1];
            }
        }

        return (from, to, opening, cleanNotes);
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
