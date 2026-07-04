using Microsoft.AspNetCore.Mvc;

namespace SnapDocs.Web.Controllers;

public class DocumentsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
