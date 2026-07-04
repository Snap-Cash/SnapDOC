using SnapDocs.Application.DTOs;

namespace SnapDocs.Application.Services;

public static class DocumentCalculator
{
    public static DocumentTotalsDto Calculate(IEnumerable<DocumentItemInputDto> items, decimal documentDiscount, decimal taxRate)
    {
        var subTotal = items.Sum(x => Math.Max(0, x.Quantity) * Math.Max(0, x.UnitPrice) - Math.Max(0, x.Discount));
        subTotal = Math.Max(0, subTotal);
        var discount = Math.Min(Math.Max(0, documentDiscount), subTotal);
        var taxable = subTotal - discount;
        var tax = Math.Round(taxable * Math.Max(0, taxRate) / 100m, 2);
        var total = taxable + tax;
        return new DocumentTotalsDto(subTotal, discount, tax, total);
    }
}
