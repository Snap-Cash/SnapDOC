using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;

namespace SnapDocs.Application.Abstractions;

public interface ISnapDocsDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<CompanyUser> CompanyUsers { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<DocumentTemplateSetting> DocumentTemplateSettings { get; }
    DbSet<Company> Companies { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Product> Products { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentItem> DocumentItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
