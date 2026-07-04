using Microsoft.EntityFrameworkCore;
using SnapDocs.Application.DTOs.Auth;
using SnapDocs.Application.Services.Auth;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Entities.Identity;
using SnapDocs.Domain.Entities.SaaS;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;

namespace SnapDocs.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SnapDocsDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(SnapDocsDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<AppUser> RegisterTenantAsync(RegisterTenantDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _db.AppUsers.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
            throw new InvalidOperationException("هذا البريد مستخدم بالفعل.");

        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(x => x.Code == dto.PlanCode, cancellationToken)
                   ?? await _db.SubscriptionPlans.FirstAsync(x => x.Code == "FREE", cancellationToken);

        var tenant = new Tenant
        {
            Name = dto.CompanyName.Trim(),
            Slug = CreateSlug(dto.CompanyName),
            PlanCode = plan.Code,
            SubscriptionStatus = SubscriptionStatus.Trial,
            TrialEndsAt = DateTime.UtcNow.AddDays(14),
            MonthlyDocumentLimit = plan.DocumentLimit
        };

        var company = new Company
        {
            Tenant = tenant,
            Name = dto.CompanyName.Trim(),
            Phone = string.Empty,
            Email = normalizedEmail,
            Address = string.Empty,
            TaxNumber = string.Empty,
            CurrencyCode = "EGP"
        };

        var user = new AppUser
        {
            TenantId = tenant.Id,
            CompanyId = company.Id,
            FullName = dto.OwnerName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _hasher.Hash(dto.Password),
            Role = UserRole.Owner,
            EmailConfirmed = true,
            IsActive = true
        };

        _db.Tenants.Add(tenant);
        _db.Companies.Add(company);
        _db.AppUsers.Add(user);
        _db.TenantSubscriptions.Add(new TenantSubscription
        {
            TenantId = tenant.Id,
            Plan = plan,
            Status = SubscriptionStatus.Trial,
            StartsAt = DateTime.UtcNow,
            TrialEndsAt = tenant.TrialEndsAt
        });

        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<AppUser?> ValidateLoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.AppUsers.FirstOrDefaultAsync(x => x.Email == email && x.IsActive, cancellationToken);
        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash)) return null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static string CreateSlug(string value)
    {
        var raw = new string(value.Trim().ToLowerInvariant().Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == ' ').ToArray());
        return raw.Replace(' ', '-').Trim('-') + "-" + DateTime.UtcNow.Ticks.ToString()[^5..];
    }
}
