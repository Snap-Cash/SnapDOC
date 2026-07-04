using System.ComponentModel.DataAnnotations;

namespace SnapDocs.Web.Models;

public class QuotationCreateViewModel
{
    [Display(Name = "العميل")]
    public Guid? CustomerId { get; set; }

    [Required]
    [Display(Name = "تاريخ العرض")]
    public DateTime DocumentDate { get; set; } = DateTime.Today;

    [Display(Name = "صالح حتى")]
    public DateTime? ValidUntil { get; set; } = DateTime.Today.AddDays(15);

    [Range(0, 999999999)]
    public decimal Discount { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 14;

    public string? Terms { get; set; }

    public List<QuotationItemViewModel> Items { get; set; } = new() { new() };
}

public class QuotationItemViewModel
{
    public Guid? ProductId { get; set; }

    [Required(ErrorMessage = "اكتب وصف البند")]
    public string Description { get; set; } = string.Empty;

    [Range(0.001, 999999999, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 999999999)]
    public decimal UnitPrice { get; set; }

    [Range(0, 999999999)]
    public decimal Discount { get; set; }
}
