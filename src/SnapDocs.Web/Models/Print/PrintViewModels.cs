using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models.Print;

public sealed class PrintDocumentViewModel
{
    public Company Company { get; set; } = new();
    public Document Document { get; set; } = new();
    public DocumentPrintSettings Settings { get; set; } = new();
    public string DocumentTitleAr { get; set; } = string.Empty;
    public string DocumentTitleEn { get; set; } = string.Empty;
    public string VerifyUrl { get; set; } = string.Empty;
    public string QrPayload { get; set; } = string.Empty;
    public string PrintedBy { get; set; } = "مدير النظام";
    public DateTime PrintedAt { get; set; } = DateTime.Now;
    public decimal Remaining => Document.Total - Document.PaidAmount;
}

public sealed class DocumentPrintSettings
{
    public DocumentType DocumentType { get; set; }
    public string TemplateName { get; set; } = "Corporate";
    public string AccentColor { get; set; } = "#2563eb";
    public bool ShowQrCode { get; set; } = true;
    public bool ShowWatermark { get; set; } = true;
    public string FooterText { get; set; } = "شكراً لتعاملكم معنا";
    public string PaperSize { get; set; } = "A4";
    public string Orientation { get; set; } = "Portrait";
}

public sealed class PrintTemplateCardViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AccentColor { get; set; } = "#2563eb";
    public bool IsCurrent { get; set; }
}
