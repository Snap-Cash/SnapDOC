using SnapDocs.Domain.Common;

namespace SnapDocs.Domain.Entities;

public class DocumentItem : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}
