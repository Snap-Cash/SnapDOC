using SnapDocs.Domain.Enums;
using SnapDocs.Web.Models.Print;

namespace SnapDocs.Web.Services.Print;

public interface IDocumentPrintService
{
    Task<PrintDocumentViewModel?> BuildAsync(Guid documentId, Guid companyId, string? baseUrl = null);
    string GetDocumentTitleAr(DocumentType type);
    string GetDocumentTitleEn(DocumentType type);
}
