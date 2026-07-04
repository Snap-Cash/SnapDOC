using Microsoft.AspNetCore.Mvc;
namespace SnapDocs.Web.Controllers;
public class SnapUIController : Controller
{
    public IActionResult Index() => View();
}
