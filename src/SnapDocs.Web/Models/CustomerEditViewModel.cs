using System.ComponentModel.DataAnnotations;

namespace SnapDocs.Web.Models;

public class CustomerEditViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "كود العميل")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "اسم العميل مطلوب")]
    [Display(Name = "اسم العميل")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "الهاتف")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "الرقم الضريبي")]
    public string? TaxNumber { get; set; }

    [Display(Name = "الرصيد الافتتاحي")]
    public decimal OpeningBalance { get; set; }

    [Display(Name = "حد الائتمان")]
    public decimal CreditLimit { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;
}
