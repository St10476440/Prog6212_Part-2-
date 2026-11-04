using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public sealed class OrganiserController : Controller
{
    public IActionResult Index() => View();
}
