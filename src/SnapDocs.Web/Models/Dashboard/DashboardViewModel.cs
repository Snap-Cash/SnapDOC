using SnapDocs.Domain.Enums;

namespace SnapDocs.Web.Models.Dashboard;

public sealed class DashboardViewModel
{
    public string CompanyName { get; set; } = "SnapDocs";
    public string PlanName { get; set; } = "Pro";
    public string SubscriptionStatus { get; set; } = "Trial";
    public int TrialDaysLeft { get; set; }

    public int TotalDocuments { get; set; }
    public int MonthDocuments { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public decimal MonthSales { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int OverdueDocuments { get; set; }
    public decimal CollectionRate { get; set; }

    public int DocumentLimit { get; set; }
    public int DocumentUsage { get; set; }
    public int CustomerLimit { get; set; }
    public int UserLimit { get; set; }

    public List<DashboardDocumentRow> RecentDocuments { get; set; } = new();
    public List<DashboardActivityRow> RecentActivities { get; set; } = new();
    public List<DashboardNotificationRow> Notifications { get; set; } = new();
    public List<DashboardQuickAction> QuickActions { get; set; } = new();
    public List<DashboardTypeMetric> TypeMetrics { get; set; } = new();
}

public sealed class DashboardDocumentRow
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string CustomerName { get; set; } = "بدون عميل";
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime DocumentDate { get; set; }
    public string ControllerName => Type switch
    {
        DocumentType.Invoice => "Invoices",
        DocumentType.Quotation => "Quotations",
        DocumentType.ReceiptVoucher => "ReceiptVouchers",
        DocumentType.PaymentVoucher => "PaymentVouchers",
        DocumentType.DeliveryNote => "DeliveryNotes",
        DocumentType.CustomerStatement => "CustomerStatements",
        _ => "Documents"
    };
}

public sealed class DashboardActivityRow
{
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class DashboardNotificationRow
{
    public string Icon { get; set; } = "🔔";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsRead { get; set; }
}

public sealed class DashboardQuickAction
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Create";
    public string Style { get; set; } = string.Empty;
}

public sealed class DashboardTypeMetric
{
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Total { get; set; }
}
