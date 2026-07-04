using System.ComponentModel.DataAnnotations;

namespace SnapDocs.Web.Models;

public class ProductsIndexViewModel
{
    public string? Search { get; set; }
    public string Status { get; set; } = "all";
    public string Type { get; set; } = "all";
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = new List<ProductListItemViewModel>();
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int ServicesCount { get; set; }
    public decimal InventoryValue { get; set; }
}

public class ProductListItemViewModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal OpeningQuantity { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsService { get; set; }
    public bool IsActive { get; set; }
    public decimal StockValue => IsService ? 0 : OpeningQuantity * CostPrice;
    public bool NeedsReorder => !IsService && ReorderLevel > 0 && OpeningQuantity <= ReorderLevel;
}

public class ProductCreateViewModel
{
    [Display(Name = "كود المنتج / الخدمة")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "اسم المنتج أو الخدمة مطلوب")]
    [Display(Name = "الاسم")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Display(Name = "الوحدة")]
    public string UnitName { get; set; } = "قطعة";

    [Display(Name = "التصنيف")]
    public string Category { get; set; } = "عام";

    [Display(Name = "سعر البيع")]
    public decimal SalePrice { get; set; }

    [Display(Name = "التكلفة")]
    public decimal CostPrice { get; set; }

    [Display(Name = "نسبة الضريبة")]
    public decimal TaxRate { get; set; } = 14;

    [Display(Name = "كمية افتتاحية")]
    public decimal OpeningQuantity { get; set; }

    [Display(Name = "حد إعادة الطلب")]
    public decimal ReorderLevel { get; set; }

    [Display(Name = "خدمة وليست منتج مخزني")]
    public bool IsService { get; set; }
}

public class ProductEditViewModel : ProductCreateViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;
}

public class ProductStudioViewModel : ProductListItemViewModel
{
    public string? Description { get; set; }
    public decimal GrossMargin => SalePrice <= 0 ? 0 : SalePrice - CostPrice;
    public decimal GrossMarginPercent => SalePrice <= 0 ? 0 : Math.Round((GrossMargin / SalePrice) * 100, 2);
    public IReadOnlyList<ProductTimelineItemViewModel> Timeline { get; set; } = new List<ProductTimelineItemViewModel>();
}

public class ProductTimelineItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Tone { get; set; } = "primary";
}
