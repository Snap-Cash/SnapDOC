using SnapDocs.Domain.Enums;

namespace SnapDocs.Application.DTOs;

public sealed class DocumentWorkflowActionDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public DocumentStatus TargetStatus { get; set; }
}
