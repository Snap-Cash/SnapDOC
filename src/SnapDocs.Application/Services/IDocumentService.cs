using SnapDocs.Application.DTOs;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Application.Services;

public interface IDocumentService
{
    Task<string> GenerateNumberAsync(Guid companyId, DocumentType type, CancellationToken cancellationToken = default);
    Task<Document> CreateAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default);
}
