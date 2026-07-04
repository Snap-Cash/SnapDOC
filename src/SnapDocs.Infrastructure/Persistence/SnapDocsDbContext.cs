using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.Abstractions;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Entities.Identity;
using SnapDocs.Domain.Entities.SaaS;

namespace SnapDocs.Infrastructure.Persistence;

public class SnapDocsDbContext : DbContext, ISnapDocsDbContext
{
    public SnapDocsDbContext(DbContextOptions<SnapDocsDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentItem> DocumentItems => Set<DocumentItem>();
    public DbSet<CashAccount> CashAccounts => Set<CashAccount>();
    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();
    public DbSet<CompanyUser> CompanyUsers => Set<CompanyUser>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<DocumentTemplateSetting> DocumentTemplateSettings => Set<DocumentTemplateSetting>();
    public DbSet<CompanyBranding> CompanyBrandings => Set<CompanyBranding>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<BillingInvoice> BillingInvoices => Set<BillingInvoice>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Tenant>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<Company>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Companies)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        builder.Entity<CompanyBranding>(entity =>
        {
            entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LogoPath).HasMaxLength(500);
            entity.Property(x => x.FaviconPath).HasMaxLength(500);
            entity.Property(x => x.LoginBackgroundPath).HasMaxLength(500);
            entity.Property(x => x.PrimaryColor).HasMaxLength(20).IsRequired();
            entity.Property(x => x.SecondaryColor).HasMaxLength(20).IsRequired();
            entity.Property(x => x.SuccessColor).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DangerColor).HasMaxLength(20).IsRequired();
            entity.Property(x => x.WarningColor).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FontFamily).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ThemeName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Radius).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FooterText).HasMaxLength(500);
            entity.Property(x => x.LoginWelcomeText).HasMaxLength(500);
            entity.HasIndex(x => x.CompanyId).IsUnique();
            entity.HasOne(x => x.Company)
                .WithOne(x => x.Branding)
                .HasForeignKey<CompanyBranding>(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            entity.Property(x => x.CreditLimit).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        });



        builder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(220).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.UnitName).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(120).IsRequired();
            entity.Property(x => x.SalePrice).HasPrecision(18, 2);
            entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            entity.Property(x => x.TaxRate).HasPrecision(18, 2);
            entity.Property(x => x.OpeningQuantity).HasPrecision(18, 3);
            entity.Property(x => x.ReorderLevel).HasPrecision(18, 3);
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        });

        builder.Entity<Document>(entity =>
        {
            entity.Property(x => x.Number).HasMaxLength(50).IsRequired();
            entity.Property(x => x.VerifyCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.SubTotal).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.TaxRate).HasPrecision(18, 2);
            entity.Property(x => x.Tax).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique();
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DocumentItem>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Document)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });



        builder.Entity<CashAccount>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(40).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        });

        builder.Entity<CashTransaction>(entity =>
        {
            entity.Property(x => x.Debit).HasPrecision(18, 2);
            entity.Property(x => x.Credit).HasPrecision(18, 2);
            entity.Property(x => x.PaymentMethod).HasMaxLength(80);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(120);
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.CashAccount)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.CashAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Document)
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CompanyUser>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Email }).IsUnique();
        });

        builder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
        });

        builder.Entity<ActivityLog>(entity =>
        {
            entity.Property(x => x.ActorName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(160).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(120).IsRequired();
        });

        builder.Entity<DocumentTemplateSetting>(entity =>
        {
            entity.Property(x => x.TemplateName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.AccentColor).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.DocumentType }).IsUnique();
        });

        builder.Entity<AppUser>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        });

        builder.Entity<PermissionDefinition>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Module).HasMaxLength(80).IsRequired();
            entity.Property(x => x.NameAr).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.Property(x => x.PermissionCode).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Role, x.PermissionCode }).IsUnique();
        });

        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.NameAr).HasMaxLength(120).IsRequired();
            entity.Property(x => x.MonthlyPrice).HasPrecision(18, 2);
            entity.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<TenantSubscription>(entity =>
        {
            entity.HasOne(x => x.Plan)
                .WithMany()
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BillingInvoice>(entity =>
        {
            entity.Property(x => x.Number).HasMaxLength(60).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.SubTotal).HasPrecision(18, 2);
            entity.Property(x => x.Tax).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasOne(x => x.Plan)
                .WithMany()
                .HasForeignKey(x => x.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SubscriptionPayment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Provider).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ReferenceNumber).HasMaxLength(120);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasOne(x => x.BillingInvoice)
                .WithMany()
                .HasForeignKey(x => x.BillingInvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
