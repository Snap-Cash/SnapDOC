using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Entities.Identity;
using SnapDocs.Domain.Entities.SaaS;
using SnapDocs.Domain.Enums;

namespace SnapDocs.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(SnapDocsDbContext db)
    {
        await db.Database.EnsureCreatedAsync();



        if (!await db.SubscriptionPlans.AnyAsync())
        {
            db.SubscriptionPlans.AddRange(
                new SubscriptionPlan { Code = "FREE", NameAr = "مجانية", NameEn = "Free", MonthlyPrice = 0, DocumentLimit = 30, UserLimit = 1, CustomerLimit = 25, HasWatermark = true },
                new SubscriptionPlan { Code = "BASIC", NameAr = "أساسية", NameEn = "Basic", MonthlyPrice = 250, DocumentLimit = 300, UserLimit = 3, CustomerLimit = 200, HasWatermark = false, CanUseWhatsApp = true },
                new SubscriptionPlan { Code = "PRO", NameAr = "احترافية", NameEn = "Pro", MonthlyPrice = 600, DocumentLimit = 2000, UserLimit = 10, CustomerLimit = 2000, HasWatermark = false, CanUseWhatsApp = true, CanUseCustomTemplates = true },
                new SubscriptionPlan { Code = "BUSINESS", NameAr = "أعمال", NameEn = "Business", MonthlyPrice = 1200, DocumentLimit = 0, UserLimit = 25, CustomerLimit = 0, HasWatermark = false, CanUseWhatsApp = true, CanUseCustomTemplates = true, CanUseApi = true }
            );
            await db.SaveChangesAsync();
        }

        var tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        if (!await db.Tenants.AnyAsync(x => x.Id == tenantId))
        {
            db.Tenants.Add(new Tenant
            {
                Id = tenantId,
                Name = "SnapDocs Demo Workspace",
                Slug = "demo",
                PlanCode = "PRO",
                SubscriptionStatus = SubscriptionStatus.Trial,
                MonthlyDocumentLimit = 250,
                MonthlyDocumentCount = 12,
                TrialEndsAt = DateTime.UtcNow.AddDays(14)
            });
        }

        if (!await db.Companies.AnyAsync(x => x.Id == companyId))
        {
            db.Companies.Add(new Company
            {
                Id = companyId,
                TenantId = tenantId,
                Name = "SnapDocs Demo Company",
                Phone = "01000000000",
                Email = "info@snapdocs.local",
                Address = "Cairo, Egypt",
                TaxNumber = "TAX-000000"
            });
        }



        if (!await db.TenantSubscriptions.AnyAsync(x => x.TenantId == tenantId))
        {
            var proPlan = await db.SubscriptionPlans.FirstAsync(x => x.Code == "PRO");
            db.TenantSubscriptions.Add(new TenantSubscription
            {
                TenantId = tenantId,
                PlanId = proPlan.Id,
                Status = SubscriptionStatus.Trial,
                StartsAt = DateTime.UtcNow,
                TrialEndsAt = DateTime.UtcNow.AddDays(14),
                AutoRenew = false
            });
        }

        if (!await db.AppUsers.AnyAsync(x => x.TenantId == tenantId))
        {
            var hasher = new SnapDocs.Infrastructure.Services.SimplePasswordHasher();
            db.AppUsers.Add(new AppUser
            {
                TenantId = tenantId,
                CompanyId = companyId,
                FullName = "مدير النظام",
                Email = "owner@snapdocs.local",
                PasswordHash = hasher.Hash("123456"),
                Role = UserRole.Owner,
                EmailConfirmed = true,
                IsActive = true
            });
        }

        if (!await db.Customers.AnyAsync(x => x.CompanyId == companyId))
        {
            db.Customers.AddRange(
                new Customer { Id = Guid.Parse("22222222-2222-2222-2222-222222222221"), CompanyId = companyId, Code = "CUS-00001", Name = "شركة الأمل للتجارة", Phone = "01012345678", OpeningBalance = 15000, CreditLimit = 50000 },
                new Customer { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), CompanyId = companyId, Code = "CUS-00002", Name = "مؤسسة النور", Phone = "01112345678", OpeningBalance = 0, CreditLimit = 25000 }
            );
        }


        if (!await db.Products.AnyAsync(x => x.CompanyId == companyId))
        {
            db.Products.AddRange(
                new Product { CompanyId = companyId, Code = "PRD-00001", Name = "اشتراك برنامج SnapDocs", Category = "برمجيات", UnitName = "اشتراك", SalePrice = 600, CostPrice = 120, TaxRate = 14, OpeningQuantity = 0, IsService = true },
                new Product { CompanyId = companyId, Code = "SRV-00001", Name = "إعداد وطباعة كشف حساب", Category = "خدمات", UnitName = "مرة", SalePrice = 150, CostPrice = 30, TaxRate = 14, OpeningQuantity = 0, IsService = true },
                new Product { CompanyId = companyId, Code = "PRD-00002", Name = "رول ورق حراري", Category = "مستلزمات", UnitName = "رول", SalePrice = 35, CostPrice = 22, TaxRate = 14, OpeningQuantity = 120, ReorderLevel = 20, IsService = false },
                new Product { CompanyId = companyId, Code = "PRD-00003", Name = "طابعة باركود", Category = "أجهزة", UnitName = "قطعة", SalePrice = 3200, CostPrice = 2600, TaxRate = 14, OpeningQuantity = 5, ReorderLevel = 2, IsService = false }
            );
        }




        if (!await db.CashAccounts.AnyAsync(x => x.CompanyId == companyId))
        {
            db.CashAccounts.AddRange(
                new CashAccount { Id = Guid.Parse("33333333-3333-3333-3333-333333333331"), CompanyId = companyId, Code = "CASH-001", Name = "الخزنة الرئيسية", Type = "Cash", CurrencyCode = "EGP", OpeningBalance = 10000, IsDefault = true },
                new CashAccount { Id = Guid.Parse("33333333-3333-3333-3333-333333333332"), CompanyId = companyId, Code = "BANK-001", Name = "حساب البنك", Type = "Bank", CurrencyCode = "EGP", OpeningBalance = 25000, IsDefault = false }
            );
        }

        if (!await db.Documents.AnyAsync(x => x.CompanyId == companyId))
        {
            var customer1Id = Guid.Parse("22222222-2222-2222-2222-222222222221");
            var customer2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

            db.Documents.AddRange(
                new Document { CompanyId = companyId, CustomerId = customer1Id, Type = DocumentType.Invoice, Status = DocumentStatus.Paid, Number = "INV-2026-0001", DocumentDate = DateTime.Today.AddDays(-8), DueDate = DateTime.Today.AddDays(7), SubTotal = 12000, Discount = 500, TaxRate = 14, Tax = 1610, Total = 13110, PaidAmount = 13110, Notes = "فاتورة تجريبية مدفوعة" },
                new Document { CompanyId = companyId, CustomerId = customer2Id, Type = DocumentType.Invoice, Status = DocumentStatus.Sent, Number = "INV-2026-0002", DocumentDate = DateTime.Today.AddDays(-3), DueDate = DateTime.Today.AddDays(10), SubTotal = 8500, Discount = 0, TaxRate = 14, Tax = 1190, Total = 9690, PaidAmount = 2000, Notes = "فاتورة تجريبية مرسلة" },
                new Document { CompanyId = companyId, CustomerId = customer1Id, Type = DocumentType.Quotation, Status = DocumentStatus.Sent, Number = "QUT-2026-0001", DocumentDate = DateTime.Today.AddDays(-2), DueDate = DateTime.Today.AddDays(12), SubTotal = 18500, Discount = 1000, TaxRate = 14, Tax = 2450, Total = 19950, PaidAmount = 0, Notes = "عرض سعر تجريبي" },
                new Document { CompanyId = companyId, CustomerId = customer2Id, Type = DocumentType.CustomerStatement, Status = DocumentStatus.Draft, Number = "STM-2026-0001", DocumentDate = DateTime.Today, SubTotal = 0, Discount = 0, TaxRate = 0, Tax = 0, Total = 26900, PaidAmount = 0, Notes = "كشف حساب تجريبي" },
                new Document { CompanyId = companyId, CustomerId = customer1Id, Type = DocumentType.ReceiptVoucher, Status = DocumentStatus.Paid, Number = "RCV-2026-0001", DocumentDate = DateTime.Today.AddDays(-1), SubTotal = 5000, Discount = 0, TaxRate = 0, Tax = 0, Total = 5000, PaidAmount = 5000, Notes = "سند قبض تجريبي" }
            );
        }



        if (!await db.CashTransactions.AnyAsync(x => x.CompanyId == companyId))
        {
            var mainCashId = Guid.Parse("33333333-3333-3333-3333-333333333331");
            var customer1Id = Guid.Parse("22222222-2222-2222-2222-222222222221");
            db.CashTransactions.AddRange(
                new CashTransaction { CompanyId = companyId, CashAccountId = mainCashId, CustomerId = customer1Id, Type = CashTransactionType.OpeningBalance, TransactionDate = DateTime.Today.AddDays(-10), Debit = 10000, Credit = 0, Description = "رصيد افتتاحي" },
                new CashTransaction { CompanyId = companyId, CashAccountId = mainCashId, CustomerId = customer1Id, Type = CashTransactionType.Receipt, TransactionDate = DateTime.Today.AddDays(-1), Debit = 5000, Credit = 0, PaymentMethod = "نقدي", ReferenceNumber = "RCV-DEMO", Description = "سند قبض تجريبي" }
            );
        }

        if (!await db.CompanyUsers.AnyAsync(x => x.CompanyId == companyId))
        {
            db.CompanyUsers.AddRange(
                new CompanyUser { CompanyId = companyId, FullName = "مدير النظام", Email = "owner@snapdocs.local", Role = UserRole.Owner, CanCreateDocuments = true, CanApproveDocuments = true, CanCancelDocuments = true, CanManageCustomers = true, CanManageSettings = true },
                new CompanyUser { CompanyId = companyId, FullName = "محاسب", Email = "accountant@snapdocs.local", Role = UserRole.Accountant, CanCreateDocuments = true, CanApproveDocuments = true, CanCancelDocuments = false, CanManageCustomers = true, CanManageSettings = false }
            );
        }

        if (!await db.Notifications.AnyAsync(x => x.CompanyId == companyId))
        {
            db.Notifications.AddRange(
                new Notification { CompanyId = companyId, Icon = "🚀", Title = "تم تجهيز مساحة العمل", Message = "يمكنك الآن إنشاء المستندات والعملاء وإدارة الاشتراك." },
                new Notification { CompanyId = companyId, Icon = "⏳", Title = "الفترة التجريبية", Message = "الفترة التجريبية تنتهي خلال 14 يومًا." }
            );
        }

        if (!await db.ActivityLogs.AnyAsync(x => x.CompanyId == companyId))
        {
            db.ActivityLogs.AddRange(
                new ActivityLog { CompanyId = companyId, ActorName = "System", Action = "تهيئة النظام", EntityName = "Workspace", Notes = "تم إنشاء بيانات تجريبية للنسخة الحالية." },
                new ActivityLog { CompanyId = companyId, ActorName = "مدير النظام", Action = "إنشاء فاتورة", EntityName = "Invoice", EntityNumber = "INV-2026-0002" },
                new ActivityLog { CompanyId = companyId, ActorName = "محاسب", Action = "إصدار سند قبض", EntityName = "ReceiptVoucher", EntityNumber = "RCV-2026-0001" }
            );
        }

        if (!await db.CompanyBrandings.AnyAsync(x => x.CompanyId == companyId))
        {
            db.CompanyBrandings.Add(new CompanyBranding
            {
                CompanyId = companyId,
                CompanyName = "SnapDocs Demo Company",
                PrimaryColor = "#2563EB",
                SecondaryColor = "#1E293B",
                SuccessColor = "#22C55E",
                DangerColor = "#EF4444",
                WarningColor = "#F59E0B",
                FontFamily = "Cairo",
                ThemeName = "Corporate",
                Radius = "22px",
                FooterText = "شكراً لاستخدام SnapDocs",
                LoginWelcomeText = "مرحباً بك في مساحة عمل SnapDocs"
            });
        }

        if (!await db.DocumentTemplateSettings.AnyAsync(x => x.CompanyId == companyId))
        {
            foreach (DocumentType type in Enum.GetValues<DocumentType>())
            {
                db.DocumentTemplateSettings.Add(new DocumentTemplateSetting { CompanyId = companyId, DocumentType = type, TemplateName = "Corporate", AccentColor = "#2563eb", FooterText = "شكراً لتعاملكم معنا" });
            }
        }

        if (!await db.BillingInvoices.AnyAsync(x => x.TenantId == tenantId))
        {
            var proPlan = await db.SubscriptionPlans.FirstAsync(x => x.Code == "PRO");
            var tax = Math.Round(proPlan.MonthlyPrice * 0.14m, 2);
            var invoice = new BillingInvoice
            {
                TenantId = tenantId,
                SubscriptionPlanId = proPlan.Id,
                Number = "BILL-202607-0001",
                Status = BillingInvoiceStatus.Issued,
                IssueDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(5),
                SubTotal = proPlan.MonthlyPrice,
                Tax = tax,
                Total = proPlan.MonthlyPrice + tax,
                PaidAmount = 0,
                CurrencyCode = "EGP",
                Notes = "فاتورة اشتراك تجريبية لباقة Pro"
            };
            db.BillingInvoices.Add(invoice);
        }

        if (!await db.SubscriptionPayments.AnyAsync(x => x.TenantId == tenantId))
        {
            db.SubscriptionPayments.Add(new SubscriptionPayment
            {
                TenantId = tenantId,
                Amount = 0,
                CurrencyCode = "EGP",
                Provider = "Manual",
                ReferenceNumber = "DEMO-PENDING",
                Status = PaymentStatus.Pending,
                Notes = "سجل دفع تجريبي مبدئي"
            });
        }

        await db.SaveChangesAsync();
    }
}
