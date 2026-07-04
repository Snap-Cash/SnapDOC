using Microsoft.AspNetCore.Mvc;

namespace SnapDocs.Web.Controllers;

public class SettingsController : Controller
{
    public IActionResult Index() => View();
}
