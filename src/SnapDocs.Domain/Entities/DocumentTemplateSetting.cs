using SnapDocs.Domain.Common;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Domain.Entities;

public class DocumentTemplateSetting : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public DocumentType DocumentType { get; set; }
    public string TemplateName { get; set; } = "Corporate";
    public string AccentColor { get; set; } = "#2563eb";
    public bool ShowQrCode { get; set; } = true;
    public bool ShowWatermark { get; set; } = true;
    public string? FooterText { get; set; }
}
