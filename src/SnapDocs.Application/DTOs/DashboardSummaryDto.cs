namespace SnapDocs.Application.DTOs;

public sealed record DashboardSummaryDto(
    int DocumentsCount,
    int CustomersCount,
    decimal SalesTotal,
    int StatementsCount
);
