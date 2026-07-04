namespace SnapDocs.Application.DTOs.SaaS;

public class TenantUsageDto
{
    public string TenantName { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int DocumentsUsed { get; set; }
    public int DocumentLimit { get; set; }
    public int UsersUsed { get; set; }
    public int UserLimit { get; set; }
    public int CustomersUsed { get; set; }
    public int CustomerLimit { get; set; }
    public DateTime? TrialEndsAt { get; set; }
}
