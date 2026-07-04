using SnapDocs.Domain.Common;

namespace SnapDocs.Domain.Entities;

public class Product : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitName { get; set; } = "قطعة";
    public string Category { get; set; } = "عام";
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TaxRate { get; set; } = 14;
    public decimal OpeningQuantity { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsService { get; set; }
    public bool IsActive { get; set; } = true;
}
