using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Participant")]
public sealed class ParticipantController : Controller
{
    public IActionResult Index() => View();
}
