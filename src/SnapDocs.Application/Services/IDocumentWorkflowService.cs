using SnapDocs.Application.DTOs;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Application.Services;

public interface IDocumentWorkflowService
{
    IReadOnlyList<DocumentWorkflowActionDto> GetAvailableActions(Document document);
    void Apply(Document document, DocumentStatus targetStatus);
}
