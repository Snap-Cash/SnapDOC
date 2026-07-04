using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models.Print;

namespace SnapDocs.Web.Services.Print;

public sealed class DocumentPrintService : IDocumentPrintService
{
    private readonly SnapDocsDbContext _db;

    public DocumentPrintService(SnapDocsDbContext db)
    {
        _db = db;
    }

    public async Task<PrintDocumentViewModel?> BuildAsync(Guid documentId, Guid companyId, string? baseUrl = null)
    {
        var document = await _db.Documents
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == documentId && x.CompanyId == companyId);

        if (document is null) return null;

        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == companyId);
        if (company is null) return null;

        var setting = await _db.DocumentTemplateSettings
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.DocumentType == document.Type);

        var verifyUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? $"/verify/{document.VerifyCode}"
            : $"{baseUrl.TrimEnd('/')}/verify/{document.VerifyCode}";

        return new PrintDocumentViewModel
        {
            Company = company,
            Document = document,
            DocumentTitleAr = GetDocumentTitleAr(document.Type),
            DocumentTitleEn = GetDocumentTitleEn(document.Type),
            VerifyUrl = verifyUrl,
            QrPayload = verifyUrl,
            Settings = new DocumentPrintSettings
            {
                DocumentType = document.Type,
                TemplateName = setting?.TemplateName ?? "Corporate",
                AccentColor = string.IsNullOrWhiteSpace(setting?.AccentColor) ? company.ThemeColor : setting!.AccentColor,
                ShowQrCode = setting?.ShowQrCode ?? true,
                ShowWatermark = setting?.ShowWatermark ?? true,
                FooterText = string.IsNullOrWhiteSpace(setting?.FooterText) ? "شكراً لتعاملكم معنا" : setting!.FooterText!
            }
        };
    }

    public string GetDocumentTitleAr(DocumentType type) => type switch
    {
        DocumentType.Invoice => "فاتورة بيع",
        DocumentType.Quotation => "عرض سعر",
        DocumentType.ReceiptVoucher => "سند قبض",
        DocumentType.PaymentVoucher => "سند صرف",
        DocumentType.DeliveryNote => "إذن تسليم",
        DocumentType.CustomerStatement => "كشف حساب عميل",
        DocumentType.CreditNote => "إشعار دائن",
        DocumentType.DebitNote => "إشعار مدين",
        _ => "مستند"
    };

    public string GetDocumentTitleEn(DocumentType type) => type switch
    {
        DocumentType.Invoice => "Sales Invoice",
        DocumentType.Quotation => "Quotation",
        DocumentType.ReceiptVoucher => "Receipt Voucher",
        DocumentType.PaymentVoucher => "Payment Voucher",
        DocumentType.DeliveryNote => "Delivery Note",
        DocumentType.CustomerStatement => "Customer Statement",
        DocumentType.CreditNote => "Credit Note",
        DocumentType.DebitNote => "Debit Note",
        _ => "Document"
    };
}
