using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.Abstractions;
using SnapDocs.Application.DTOs;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly ISnapDocsDbContext _db;

    public DocumentService(ISnapDocsDbContext db) => _db = db;

    public async Task<string> GenerateNumberAsync(Guid companyId, DocumentType type, CancellationToken cancellationToken = default)
    {
        var prefix = type switch
        {
            DocumentType.Invoice => "INV",
            DocumentType.Quotation => "QUO",
            DocumentType.ReceiptVoucher => "REC",
            DocumentType.PaymentVoucher => "PAY",
            DocumentType.DeliveryNote => "DLV",
            DocumentType.CustomerStatement => "STM",
            DocumentType.CreditNote => "CRN",
            DocumentType.DebitNote => "DBN",
            _ => "DOC"
        };

        var year = DateTime.Today.Year;
        var count = await _db.Documents.CountAsync(x => x.CompanyId == companyId && x.Type == type && x.DocumentDate.Year == year, cancellationToken);
        return $"{prefix}-{year}-{count + 1:00000}";
    }

    public async Task<Document> CreateAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var cleanItems = dto.Items.Where(x => !string.IsNullOrWhiteSpace(x.Description)).ToList();
        if (!cleanItems.Any()) cleanItems.Add(new DocumentItemInputDto { Description = "بند افتراضي", Quantity = 1, UnitPrice = 0 });

        var totals = DocumentCalculator.Calculate(cleanItems, dto.Discount, dto.TaxRate);

        var document = new Document
        {
            CompanyId = dto.CompanyId,
            CustomerId = dto.CustomerId,
            Type = dto.Type,
            Status = DocumentStatus.Draft,
            Number = await GenerateNumberAsync(dto.CompanyId, dto.Type, cancellationToken),
            DocumentDate = dto.DocumentDate,
            DueDate = dto.DueDate,
            Discount = totals.Discount,
            TaxRate = dto.TaxRate,
            SubTotal = totals.SubTotal,
            Tax = totals.Tax,
            Total = totals.Total,
            Notes = dto.Notes,
            Items = cleanItems.Select(x => new DocumentItem
            {
                Description = x.Description.Trim(),
                Quantity = Math.Max(0, x.Quantity),
                UnitPrice = Math.Max(0, x.UnitPrice),
                Discount = Math.Max(0, x.Discount),
                LineTotal = Math.Max(0, x.Quantity) * Math.Max(0, x.UnitPrice) - Math.Max(0, x.Discount)
            }).ToList()
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync(cancellationToken);
        return document;
    }
}
