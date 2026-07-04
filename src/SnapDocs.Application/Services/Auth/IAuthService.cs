using SnapDocs.Application.DTOs.Auth;
using SnapDocs.Domain.Entities.Identity;

namespace SnapDocs.Application.Services.Auth;

public interface IAuthService
{
    Task<AppUser> RegisterTenantAsync(RegisterTenantDto dto, CancellationToken cancellationToken = default);
    Task<AppUser?> ValidateLoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
