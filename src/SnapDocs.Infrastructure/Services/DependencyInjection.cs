using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SnapDocs.Application.Abstractions;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Application.Services.Auth;
using SnapDocs.Application.Services.SaaS;

namespace SnapDocs.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SnapDocsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISnapDocsDbContext>(provider => provider.GetRequiredService<SnapDocsDbContext>());
        services.AddScoped<IPasswordHasher, SimplePasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        return services;
    }
}
