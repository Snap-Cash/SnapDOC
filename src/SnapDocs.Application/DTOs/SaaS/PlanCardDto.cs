namespace SnapDocs.Application.DTOs.SaaS;

public class PlanCardDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int DocumentLimit { get; set; }
    public int UserLimit { get; set; }
    public int CustomerLimit { get; set; }
    public bool HasWatermark { get; set; }
    public bool CanUseWhatsApp { get; set; }
    public bool CanUseCustomTemplates { get; set; }
    public bool CanUseApi { get; set; }
    public bool IsCurrent { get; set; }
}
