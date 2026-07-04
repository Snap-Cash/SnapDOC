namespace SnapDocs.Application.DTOs.Auth;

public class RegisterTenantDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PlanCode { get; set; } = "FREE";
}
