using SnapDocs.Domain.Entities;

namespace SnapDocs.Web.Models;

public class CustomersIndexViewModel
{
    public string? Search { get; set; }
    public string Status { get; set; } = "all";
    public IReadOnlyList<CustomerListItemViewModel> Customers { get; set; } = new List<CustomerListItemViewModel>();
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal TotalOpeningBalance { get; set; }
    public decimal TotalCreditLimit { get; set; }
}

public class CustomerListItemViewModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public int DocumentsCount { get; set; }
    public decimal TotalInvoices { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal CurrentBalance => OpeningBalance + TotalInvoices - TotalPaid;
}
