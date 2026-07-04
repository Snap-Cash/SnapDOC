using Microsoft.AspNetCore.Mvc;

namespace SnapDocs.Web.Controllers;

public class PwaController : Controller
{
    public IActionResult Index() => View();
}
