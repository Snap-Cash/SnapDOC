using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models;

public sealed class DocumentStudioViewModel
{
    public Document Document { get; set; } = new();
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Icon { get; set; } = "📄";
    public string AccentClass { get; set; } = "blue";
    public string ControllerName { get; set; } = "Documents";
    public string CreateAction { get; set; } = "Create";
    public string DetailsAction { get; set; } = "Details";
    public string PrintAction { get; set; } = "Document";
    public string PrintController { get; set; } = "Print";
    public string VerifyUrl { get; set; } = string.Empty;
    public string WhatsAppText { get; set; } = string.Empty;
    public decimal Remaining => Document.Total - Document.PaidAmount;
    public bool HasCustomer => Document.Customer is not null;
    public IReadOnlyList<DocumentStudioTab> Tabs { get; set; } = Array.Empty<DocumentStudioTab>();
    public IReadOnlyList<DocumentStudioMetric> Metrics { get; set; } = Array.Empty<DocumentStudioMetric>();
    public IReadOnlyList<DocumentStudioTimelineItem> Timeline { get; set; } = Array.Empty<DocumentStudioTimelineItem>();
}

public sealed class DocumentStudioTab
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public sealed class DocumentStudioMetric
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
    public string Tone { get; set; } = "neutral";
}

public sealed class DocumentStudioTimelineItem
{
    public string Icon { get; set; } = "•";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
}

public static class DocumentStudioCatalog
{
    public static DocumentStudioViewModel Create(Document document, string verifyUrl = "")
    {
        var metadata = GetMetadata(document.Type);
        var remaining = document.Total - document.PaidAmount;

        return new DocumentStudioViewModel
        {
            Document = document,
            TitleAr = metadata.TitleAr,
            TitleEn = metadata.TitleEn,
            Icon = metadata.Icon,
            AccentClass = metadata.AccentClass,
            ControllerName = metadata.ControllerName,
            VerifyUrl = verifyUrl,
            WhatsAppText = $"{metadata.TitleAr} رقم {document.Number} بقيمة {document.Total:N2}. كود التحقق: {document.VerifyCode}",
            Tabs = BuildTabs(document.Type),
            Metrics = new[]
            {
                new DocumentStudioMetric { Label = "الإجمالي", Value = document.Total.ToString("N2"), Hint = "صافي المستند", Tone = "primary" },
                new DocumentStudioMetric { Label = "المدفوع", Value = document.PaidAmount.ToString("N2"), Hint = "إجمالي التحصيل", Tone = "success" },
                new DocumentStudioMetric { Label = "المتبقي", Value = remaining.ToString("N2"), Hint = remaining > 0 ? "مستحق" : "مغلق", Tone = remaining > 0 ? "danger" : "success" },
                new DocumentStudioMetric { Label = "البنود", Value = document.Items.Count.ToString(), Hint = "عدد السطور", Tone = "neutral" }
            },
            Timeline = BuildTimeline(document)
        };
    }

    public static (string TitleAr, string TitleEn, string Icon, string AccentClass, string ControllerName) GetMetadata(DocumentType type)
    {
        return type switch
        {
            DocumentType.Invoice => ("فاتورة بيع", "Invoice", "🧾", "blue", "Invoices"),
            DocumentType.Quotation => ("عرض سعر", "Quotation", "💬", "purple", "Quotations"),
            DocumentType.ReceiptVoucher => ("سند قبض", "Receipt Voucher", "📥", "green", "ReceiptVouchers"),
            DocumentType.PaymentVoucher => ("سند صرف", "Payment Voucher", "📤", "orange", "PaymentVouchers"),
            DocumentType.DeliveryNote => ("إذن تسليم", "Delivery Note", "📦", "indigo", "DeliveryNotes"),
            DocumentType.CustomerStatement => ("كشف حساب", "Statement", "📋", "slate", "CustomerStatements"),
            DocumentType.CreditNote => ("إشعار دائن", "Credit Note", "🟢", "green", "CreditNotes"),
            DocumentType.DebitNote => ("إشعار مدين", "Debit Note", "🔴", "red", "DebitNotes"),
            _ => ("مستند", "Document", "📄", "blue", "Documents")
        };
    }

    private static IReadOnlyList<DocumentStudioTab> BuildTabs(DocumentType type)
    {
        var tabs = new List<DocumentStudioTab>
        {
            new() { Key = "overview", Label = "Overview", Icon = "🏠" },
            new() { Key = "items", Label = type == DocumentType.CustomerStatement ? "Transactions" : "Items", Icon = "☷" },
            new() { Key = "payment", Label = "Payment", Icon = "💳" },
            new() { Key = "notes", Label = "Notes", Icon = "📝" },
            new() { Key = "history", Label = "History", Icon = "🕘" },
            new() { Key = "print", Label = "Print", Icon = "🖨️" }
        };
        return tabs;
    }

    private static IReadOnlyList<DocumentStudioTimelineItem> BuildTimeline(Document document)
    {
        var items = new List<DocumentStudioTimelineItem>
        {
            new() { Icon = "✨", Title = "تم إنشاء المستند", Description = $"تم إنشاء {document.Number} كمسودة.", Date = document.CreatedAtUtc.ToLocalTime() },
            new() { Icon = "🏷️", Title = "الحالة الحالية", Description = $"الحالة الآن: {document.Status}.", Date = DateTime.Now }
        };

        if (document.PaidAmount > 0)
        {
            items.Add(new DocumentStudioTimelineItem { Icon = "💳", Title = "تم تسجيل مدفوعات", Description = $"المدفوع حتى الآن {document.PaidAmount:N2}.", Date = DateTime.Now });
        }

        if (!string.IsNullOrWhiteSpace(document.VerifyCode))
        {
            items.Add(new DocumentStudioTimelineItem { Icon = "🔐", Title = "كود تحقق جاهز", Description = $"Verify Code: {document.VerifyCode}", Date = DateTime.Now });
        }

        return items;
    }
}
