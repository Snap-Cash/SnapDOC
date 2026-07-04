namespace SnapDocs.Web.Models;

public class PaymentVoucherCreateViewModel
{
    public Guid? CustomerId { get; set; }
    public Guid? CashAccountId { get; set; }
    public DateTime VoucherDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "نقدي";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
