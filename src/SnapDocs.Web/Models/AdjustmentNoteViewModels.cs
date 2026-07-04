namespace SnapDocs.Web.Models;

public class CreditNoteCreateViewModel
{
    public Guid? CustomerId { get; set; }
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = "تسوية رصيد / خصم / مرتجع";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class DebitNoteCreateViewModel
{
    public Guid? CustomerId { get; set; }
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = "تسوية رصيد / مصروف إضافي";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
