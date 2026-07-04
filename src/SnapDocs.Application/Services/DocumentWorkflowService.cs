using SnapDocs.Application.DTOs;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Application.Services;

public sealed class DocumentWorkflowService : IDocumentWorkflowService
{
    public IReadOnlyList<DocumentWorkflowActionDto> GetAvailableActions(Document document)
    {
        var actions = new List<DocumentWorkflowActionDto>();

        if (document.Status == DocumentStatus.Draft)
        {
            actions.Add(Action("Sent", "اعتماد/إرسال", "primary", DocumentStatus.Sent));
            actions.Add(Action("Cancelled", "إلغاء", "danger", DocumentStatus.Cancelled));
        }

        if (document.Status == DocumentStatus.Sent || document.Status == DocumentStatus.Overdue)
        {
            if (document.Type is DocumentType.Invoice or DocumentType.ReceiptVoucher)
                actions.Add(Action("Paid", "تحصيل/مدفوع", "success", DocumentStatus.Paid));

            if (document.Type == DocumentType.Invoice)
                actions.Add(Action("Overdue", "تعليم كمتأخر", "warning", DocumentStatus.Overdue));

            actions.Add(Action("Cancelled", "إلغاء", "danger", DocumentStatus.Cancelled));
        }

        if (document.Status == DocumentStatus.Cancelled)
            actions.Add(Action("Draft", "إرجاع لمسودة", "", DocumentStatus.Draft));

        return actions;
    }

    public void Apply(Document document, DocumentStatus targetStatus)
    {
        document.Status = targetStatus;
        document.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static DocumentWorkflowActionDto Action(string code, string label, string cssClass, DocumentStatus targetStatus) => new()
    {
        Code = code,
        Label = label,
        CssClass = cssClass,
        TargetStatus = targetStatus
    };
}
