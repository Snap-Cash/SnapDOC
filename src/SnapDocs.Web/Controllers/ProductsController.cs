using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class ProductsController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;

    public ProductsController(SnapDocsDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, string status = "all", string type = "all")
    {
        var query = _db.Products.AsNoTracking().Where(x => x.CompanyId == DemoCompanyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term) || x.Category.Contains(term) ||
                                     (x.Description != null && x.Description.Contains(term)));
        }

        query = status switch
        {
            "active" => query.Where(x => x.IsActive),
            "inactive" => query.Where(x => !x.IsActive),
            _ => query
        };

        query = type switch
        {
            "product" => query.Where(x => !x.IsService),
            "service" => query.Where(x => x.IsService),
            _ => query
        };

        var products = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ProductListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                UnitName = x.UnitName,
                Category = x.Category,
                SalePrice = x.SalePrice,
                CostPrice = x.CostPrice,
                TaxRate = x.TaxRate,
                OpeningQuantity = x.OpeningQuantity,
                ReorderLevel = x.ReorderLevel,
                IsService = x.IsService,
                IsActive = x.IsActive
            })
            .ToListAsync();

        var allProducts = await _db.Products.AsNoTracking().Where(x => x.CompanyId == DemoCompanyId).ToListAsync();

        var model = new ProductsIndexViewModel
        {
            Search = search,
            Status = status,
            Type = type,
            Products = products,
            TotalProducts = allProducts.Count,
            ActiveProducts = allProducts.Count(x => x.IsActive),
            ServicesCount = allProducts.Count(x => x.IsService),
            InventoryValue = allProducts.Where(x => !x.IsService).Sum(x => x.OpeningQuantity * x.CostPrice)
        };

        return View(model);
    }

    public IActionResult Create() => View(new ProductCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var count = await _db.Products.CountAsync(x => x.CompanyId == DemoCompanyId);
        var prefix = model.IsService ? "SRV" : "PRD";
        var code = string.IsNullOrWhiteSpace(model.Code) ? $"{prefix}-{count + 1:00000}" : model.Code.Trim();

        var duplicate = await _db.Products.AnyAsync(x => x.CompanyId == DemoCompanyId && x.Code == code);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Code), "الكود مستخدم بالفعل");
            return View(model);
        }

        var product = new Product
        {
            CompanyId = DemoCompanyId,
            Code = code,
            Name = model.Name.Trim(),
            Description = model.Description,
            UnitName = string.IsNullOrWhiteSpace(model.UnitName) ? "قطعة" : model.UnitName.Trim(),
            Category = string.IsNullOrWhiteSpace(model.Category) ? "عام" : model.Category.Trim(),
            SalePrice = model.SalePrice,
            CostPrice = model.CostPrice,
            TaxRate = model.TaxRate,
            OpeningQuantity = model.IsService ? 0 : model.OpeningQuantity,
            ReorderLevel = model.IsService ? 0 : model.ReorderLevel,
            IsService = model.IsService
        };

        _db.Products.Add(product);
        _db.ActivityLogs.Add(new ActivityLog
        {
            CompanyId = DemoCompanyId,
            ActorName = "System",
            Action = product.IsService ? "إنشاء خدمة" : "إنشاء منتج",
            EntityName = "Product",
            EntityNumber = product.Code,
            Notes = $"تم إنشاء {(product.IsService ? "الخدمة" : "المنتج")} {product.Name}"
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إضافة المنتج / الخدمة بنجاح";
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.Id == id);
        if (product is null) return NotFound();

        return View(new ProductEditViewModel
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            UnitName = product.UnitName,
            Category = product.Category,
            SalePrice = product.SalePrice,
            CostPrice = product.CostPrice,
            TaxRate = product.TaxRate,
            OpeningQuantity = product.OpeningQuantity,
            ReorderLevel = product.ReorderLevel,
            IsService = product.IsService,
            IsActive = product.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var product = await _db.Products.FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.Id == id);
        if (product is null) return NotFound();

        var code = string.IsNullOrWhiteSpace(model.Code) ? product.Code : model.Code.Trim();
        var duplicate = await _db.Products.AnyAsync(x => x.CompanyId == DemoCompanyId && x.Id != id && x.Code == code);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Code), "الكود مستخدم بالفعل");
            return View(model);
        }

        product.Code = code;
        product.Name = model.Name.Trim();
        product.Description = model.Description;
        product.UnitName = string.IsNullOrWhiteSpace(model.UnitName) ? "قطعة" : model.UnitName.Trim();
        product.Category = string.IsNullOrWhiteSpace(model.Category) ? "عام" : model.Category.Trim();
        product.SalePrice = model.SalePrice;
        product.CostPrice = model.CostPrice;
        product.TaxRate = model.TaxRate;
        product.OpeningQuantity = model.IsService ? 0 : model.OpeningQuantity;
        product.ReorderLevel = model.IsService ? 0 : model.ReorderLevel;
        product.IsService = model.IsService;
        product.IsActive = model.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        _db.ActivityLogs.Add(new ActivityLog
        {
            CompanyId = DemoCompanyId,
            ActorName = "System",
            Action = "تعديل منتج / خدمة",
            EntityName = "Product",
            EntityNumber = product.Code,
            Notes = $"تم تعديل {product.Name}"
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "تم حفظ بيانات المنتج / الخدمة";
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyId == DemoCompanyId && x.Id == id);
        if (product is null) return NotFound();

        var model = new ProductStudioViewModel
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            UnitName = product.UnitName,
            Category = product.Category,
            SalePrice = product.SalePrice,
            CostPrice = product.CostPrice,
            TaxRate = product.TaxRate,
            OpeningQuantity = product.OpeningQuantity,
            ReorderLevel = product.ReorderLevel,
            IsService = product.IsService,
            IsActive = product.IsActive,
            Timeline = new List<ProductTimelineItemViewModel>
            {
                new() { Title = "إنشاء الكارت", Description = $"تم إنشاء {product.Name}", Date = product.CreatedAtUtc, Tone = "primary" },
                new() { Title = "سعر البيع", Description = $"السعر الحالي {product.SalePrice:N2}", Date = DateTime.Today, Tone = "success" },
                new() { Title = product.IsService ? "نوع الخدمة" : "المخزون الافتتاحي", Description = product.IsService ? "هذا البند خدمة غير مخزنية" : $"الكمية الافتتاحية {product.OpeningQuantity:N3}", Date = DateTime.Today, Tone = product.IsService ? "primary" : "warning" }
            }
        };

        return View(model);
    }
}
