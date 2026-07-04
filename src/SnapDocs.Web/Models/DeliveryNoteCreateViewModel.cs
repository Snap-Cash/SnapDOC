namespace SnapDocs.Web.Models;

public class DeliveryNoteCreateViewModel
{
    public Guid? CustomerId { get; set; }
    public DateTime DeliveryDate { get; set; } = DateTime.Today;
    public string? DeliveryAddress { get; set; }
    public string? ReceiverName { get; set; }
    public string? Notes { get; set; }
    public List<DeliveryNoteItemViewModel> Items { get; set; } = new() { new() };
}

public class DeliveryNoteItemViewModel
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
}
