using Microsoft.AspNetCore.Http;

namespace SnapDocs.Web.Models;

public class BrandingViewModel
{
    public Guid? Id { get; set; }
    public Guid CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? CurrentLogoPath { get; set; }
    public string? CurrentFaviconPath { get; set; }
    public string? CurrentLoginBackgroundPath { get; set; }

    public IFormFile? Logo { get; set; }
    public IFormFile? Favicon { get; set; }
    public IFormFile? LoginBackground { get; set; }

    public string PrimaryColor { get; set; } = "#2563EB";
    public string SecondaryColor { get; set; } = "#1E293B";
    public string SuccessColor { get; set; } = "#22C55E";
    public string DangerColor { get; set; } = "#EF4444";
    public string WarningColor { get; set; } = "#F59E0B";

    public string FontFamily { get; set; } = "Cairo";
    public string ThemeName { get; set; } = "Corporate";
    public string Radius { get; set; } = "22px";
    public bool DarkMode { get; set; }
    public bool WhiteLabel { get; set; }

    public string? FooterText { get; set; }
    public string? LoginWelcomeText { get; set; }

    public IReadOnlyList<string> Fonts { get; set; } = new[] { "Cairo", "Tajawal", "IBM Plex Sans Arabic", "Segoe UI", "Inter" };
    public IReadOnlyList<string> Themes { get; set; } = new[] { "Corporate", "Classic", "Modern", "Minimal", "Retail", "Medical", "Restaurant", "Construction" };
}
