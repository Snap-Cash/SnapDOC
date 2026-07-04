using System.ComponentModel.DataAnnotations;

namespace SnapDocs.Web.Models.Auth;

public class RegisterTenantViewModel
{
    [Required]
    public string CompanyName { get; set; } = string.Empty;
    [Required]
    public string OwnerName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    public string PlanCode { get; set; } = "FREE";
}
