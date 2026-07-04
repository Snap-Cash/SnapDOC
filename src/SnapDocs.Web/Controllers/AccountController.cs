using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapDocs.Application.DTOs.Auth;
using SnapDocs.Application.Services.Auth;
using SnapDocs.Web.Models.Auth;

namespace SnapDocs.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService) => _authService = authService;

    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _authService.ValidateLoginAsync(new LoginDto { Email = model.Email, Password = model.Password });
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة.");
            return View(model);
        }

        await SignInAsync(user.Id, user.TenantId, user.CompanyId, user.FullName, user.Email, user.Role.ToString());
        return LocalRedirect(string.IsNullOrWhiteSpace(model.ReturnUrl) ? "/" : model.ReturnUrl);
    }

    public IActionResult Register() => View(new RegisterTenantViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterTenantViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var user = await _authService.RegisterTenantAsync(new RegisterTenantDto
            {
                CompanyName = model.CompanyName,
                OwnerName = model.OwnerName,
                Email = model.Email,
                Password = model.Password,
                PlanCode = model.PlanCode
            });
            await SignInAsync(user.Id, user.TenantId, user.CompanyId, user.FullName, user.Email, user.Role.ToString());
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("SnapDocsCookie");
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private async Task SignInAsync(Guid userId, Guid tenantId, Guid companyId, string name, string email, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("TenantId", tenantId.ToString()),
            new("CompanyId", companyId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "SnapDocsCookie");
        await HttpContext.SignInAsync("SnapDocsCookie", new ClaimsPrincipal(identity));
    }
}
