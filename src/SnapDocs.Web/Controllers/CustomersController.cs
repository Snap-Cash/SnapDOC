using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class CustomersController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;

    public CustomersController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, string status = "all")
    {
        var query = _db.Customers
            .AsNoTracking()
            .Include(x => x.Documents)
            .Where(x => x.CompanyId == DemoCompanyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term) ||
                                     (x.Phone != null && x.Phone.Contains(term)) ||
                                     (x.Email != null && x.Email.Contains(term)));
        }

        query = status switch
        {
            "active" => query.Where(x => x.IsActive),
            "inactive" => query.Where(x => !x.IsActive),
            _ => query
        };

        var customers = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new CustomerListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                OpeningBalance = x.OpeningBalance,
                CreditLimit = x.CreditLimit,
                IsActive = x.IsActive,
                DocumentsCount = x.Documents.Count,
                TotalInvoices = x.Documents
                    .Where(d => d.Type == DocumentType.Invoice)
                    .Sum(d => d.Total),
                TotalPaid = x.Documents.Sum(d => d.PaidAmount)
            })
            .ToListAsync();

        var allCustomers = await _db.Customers
            .AsNoTracking()
            .Where(x => x.CompanyId == DemoCompanyId)
            .ToListAsync();

        var model = new CustomersIndexViewModel
        {
            Search = search,
            Status = status,
            Customers = customers,
            TotalCustomers = allCustomers.Count,
            ActiveCustomers = allCustomers.Count(x => x.IsActive),
            TotalOpeningBalance = allCustomers.Sum(x => x.OpeningBalance),
            TotalCreditLimit = allCustomers.Sum(x => x.CreditLimit)
        };

        return View(model);
    }

    public IActionResult Create() => View(new CustomerCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var count = await _db.Customers.CountAsync(x => x.CompanyId == DemoCompanyId);
        var code = string.IsNullOrWhiteSpace(model.Code) ? $"CUS-{count + 1:00000}" : model.Code.Trim();

        var duplicate = await _db.Customers.AnyAsync(x => x.CompanyId == DemoCompanyId && x.Code == code);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Code), "كود العميل مستخدم بالفعل");
            return View(model);
        }

        var customer = new Customer
        {
            CompanyId = DemoCompanyId,
            Code = code,
            Name = model.Name.Trim(),
            Phone = model.Phone,
            Email = model.Email,
            Address = model.Address,
            TaxNumber = model.TaxNumber,
            OpeningBalance = model.OpeningBalance,
            CreditLimit = model.CreditLimit
        };

        _db.Customers.Add(customer);
        _db.ActivityLogs.Add(new ActivityLog
        {
            CompanyId = DemoCompanyId,
            ActorName = "System",
            Action = "إنشاء عميل",
            EntityName = "Customer",
            EntityNumber = customer.Code,
            Notes = $"تم إنشاء العميل {customer.Name}"
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة العميل بنجاح";
        return RedirectToAction(nameof(Details), new { id = customer.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.Id == id);
        if (customer is null) return NotFound();

        return View(new CustomerEditViewModel
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            TaxNumber = customer.TaxNumber,
            OpeningBalance = customer.OpeningBalance,
            CreditLimit = customer.CreditLimit,
            IsActive = customer.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CustomerEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var customer = await _db.Customers.FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.Id == id);
        if (customer is null) return NotFound();

        var code = string.IsNullOrWhiteSpace(model.Code) ? customer.Code : model.Code.Trim();
        var duplicate = await _db.Customers.AnyAsync(x => x.CompanyId == DemoCompanyId && x.Id != id && x.Code == code);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Code), "كود العميل مستخدم بالفعل");
            return View(model);
        }

        customer.Code = code;
        customer.Name = model.Name.Trim();
        customer.Phone = model.Phone;
        customer.Email = model.Email;
        customer.Address = model.Address;
        customer.TaxNumber = model.TaxNumber;
        customer.OpeningBalance = model.OpeningBalance;
        customer.CreditLimit = model.CreditLimit;
        customer.IsActive = model.IsActive;

        _db.ActivityLogs.Add(new ActivityLog
        {
            CompanyId = DemoCompanyId,
            ActorName = "System",
            Action = "تعديل عميل",
            EntityName = "Customer",
            EntityNumber = customer.Code,
            Notes = $"تم تعديل بيانات العميل {customer.Name}"
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "تم حفظ بيانات العميل";
        return RedirectToAction(nameof(Details), new { id = customer.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.Id == id);

        if (customer is null) return NotFound();

        var documents = customer.Documents
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToList();

        var documentRows = documents
            .Select(x => new CustomerDocumentRowViewModel
            {
                Id = x.Id,
                Number = x.Number,
                Type = x.Type,
                Status = x.Status,
                DocumentDate = x.DocumentDate,
                DueDate = x.DueDate,
                Total = x.Total,
                PaidAmount = x.PaidAmount,
                Notes = x.Notes
            })
            .ToList();

        var invoiceRows = documentRows.Where(x => x.Type == DocumentType.Invoice).ToList();
        var receiptRows = documentRows.Where(x => x.Type == DocumentType.ReceiptVoucher).ToList();
        var statementRows = documentRows.Where(x => x.Type == DocumentType.CustomerStatement).ToList();
        var totalInvoices = invoiceRows.Sum(x => x.Total);
        var totalReceipts = documents
            .Where(x => x.Type == DocumentType.ReceiptVoucher)
            .Sum(x => x.Total + x.PaidAmount);

        var timeline = documents.Take(12).Select(x => new CustomerTimelineItemViewModel
        {
            Title = GetDocumentTypeName(x.Type),
            Description = $"{x.Number} بقيمة {x.Total:N2}",
            Date = x.DocumentDate,
            Icon = GetDocumentTypeIcon(x.Type),
            Tone = x.Status == DocumentStatus.Paid ? "success" : x.Status == DocumentStatus.Overdue ? "danger" : x.Status == DocumentStatus.Cancelled ? "muted" : "primary"
        }).ToList();

        timeline.Insert(0, new CustomerTimelineItemViewModel
        {
            Title = "فتح ملف العميل",
            Description = $"تم إنشاء ملف العميل برصيد افتتاحي {customer.OpeningBalance:N2}",
            Date = customer.CreatedAtUtc,
            Tone = "muted",
            Icon = "👤"
        });

        var model = new CustomerStudioViewModel
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            TaxNumber = customer.TaxNumber,
            IsActive = customer.IsActive,
            CreatedAtUtc = customer.CreatedAtUtc,
            OpeningBalance = customer.OpeningBalance,
            CreditLimit = customer.CreditLimit,
            TotalInvoices = totalInvoices,
            TotalReceipts = totalReceipts,
            TotalPayments = documents.Where(x => x.Type == DocumentType.PaymentVoucher).Sum(x => x.Total),
            TotalQuotations = documents.Where(x => x.Type == DocumentType.Quotation).Sum(x => x.Total),
            TotalStatements = statementRows.Sum(x => x.Total),
            TotalDocuments = documents.Sum(x => x.Total),
            AverageInvoice = invoiceRows.Any() ? invoiceRows.Average(x => x.Total) : 0,
            DocumentsCount = documents.Count,
            InvoiceCount = invoiceRows.Count,
            QuotationCount = documents.Count(x => x.Type == DocumentType.Quotation),
            StatementCount = statementRows.Count,
            ReceiptCount = receiptRows.Count,
            PaymentCount = documents.Count(x => x.Type == DocumentType.PaymentVoucher),
            DraftCount = documents.Count(x => x.Status == DocumentStatus.Draft),
            SentCount = documents.Count(x => x.Status == DocumentStatus.Sent),
            PaidCount = documents.Count(x => x.Status == DocumentStatus.Paid),
            OverdueCount = documents.Count(x => x.Status == DocumentStatus.Overdue),
            LastDocumentDate = documents.FirstOrDefault()?.DocumentDate,
            LastPaymentDate = documents
                .Where(x => x.Type == DocumentType.ReceiptVoucher || x.PaidAmount > 0)
                .OrderByDescending(x => x.DocumentDate)
                .Select(x => (DateTime?)x.DocumentDate)
                .FirstOrDefault(),
            Aging = BuildAging(invoiceRows),
            RecentDocuments = documentRows.Take(30).ToList(),
            Invoices = invoiceRows.Take(20).ToList(),
            Statements = statementRows.Take(12).ToList(),
            Receipts = receiptRows.Take(20).ToList(),
            Timeline = timeline,
            DocumentTypeSummary = documents
                .GroupBy(x => x.Type)
                .Select(g => new CustomerDocumentTypeSummaryViewModel
                {
                    Type = g.Key,
                    Name = GetDocumentTypeName(g.Key),
                    Icon = GetDocumentTypeIcon(g.Key),
                    Count = g.Count(),
                    Total = g.Sum(d => d.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToList()
        };

        return View(model);
    }

    private static CustomerAgingViewModel BuildAging(IEnumerable<CustomerDocumentRowViewModel> invoices)
    {
        var today = DateTime.Today;
        var aging = new CustomerAgingViewModel();

        foreach (var invoice in invoices.Where(x => x.Balance > 0 && x.Status != DocumentStatus.Cancelled))
        {
            var dueDate = invoice.DueDate ?? invoice.DocumentDate;
            var days = (today - dueDate.Date).Days;

            if (days <= 0) aging.Current += invoice.Balance;
            else if (days <= 30) aging.Days1To30 += invoice.Balance;
            else if (days <= 60) aging.Days31To60 += invoice.Balance;
            else if (days <= 90) aging.Days61To90 += invoice.Balance;
            else aging.Over90 += invoice.Balance;
        }

        return aging;
    }

    private static string GetDocumentTypeName(DocumentType type) => type switch
    {
        DocumentType.Invoice => "فاتورة بيع",
        DocumentType.Quotation => "عرض سعر",
        DocumentType.CustomerStatement => "كشف حساب",
        DocumentType.ReceiptVoucher => "سند قبض",
        DocumentType.PaymentVoucher => "سند صرف",
        DocumentType.DeliveryNote => "إذن تسليم",
        DocumentType.CreditNote => "إشعار دائن",
        DocumentType.DebitNote => "إشعار مدين",
        _ => type.ToString()
    };

    private static string GetDocumentTypeIcon(DocumentType type) => type switch
    {
        DocumentType.Invoice => "🧾",
        DocumentType.Quotation => "💬",
        DocumentType.CustomerStatement => "📋",
        DocumentType.ReceiptVoucher => "📥",
        DocumentType.PaymentVoucher => "📤",
        DocumentType.DeliveryNote => "📦",
        DocumentType.CreditNote => "🟢",
        DocumentType.DebitNote => "🔴",
        _ => "📄"
    };
}
