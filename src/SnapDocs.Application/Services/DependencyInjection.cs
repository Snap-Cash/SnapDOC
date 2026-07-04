using Microsoft.Extensions.DependencyInjection;

namespace SnapDocs.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentWorkflowService, DocumentWorkflowService>();
        return services;
    }
}
