using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SnapDocs.Web.Controllers;

[Authorize(Roles = "Owner,Admin")]
public class PermissionsController : Controller
{
    public IActionResult Index() => View();
}
