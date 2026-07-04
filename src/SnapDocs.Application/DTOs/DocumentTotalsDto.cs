namespace SnapDocs.Application.DTOs;

public record DocumentTotalsDto(decimal SubTotal, decimal Discount, decimal Tax, decimal Total);
