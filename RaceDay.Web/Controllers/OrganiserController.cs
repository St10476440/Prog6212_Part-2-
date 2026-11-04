using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public sealed class OrganiserController(
    IHttpClientFactory clients,
    ILogger<OrganiserController> logger) : Controller
{
    private readonly HttpClient _api = clients.CreateClient("RaceDayApi");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            using var response = await _api.GetAsync("api/events/mine");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Your events could not be loaded. Please try again.";
                return View(new List<EventListItemViewModel>());
            }

            var events = await response.Content.ReadFromJsonAsync<List<EventListItemViewModel>>()
                ?? new List<EventListItemViewModel>();
            return View(events);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not load organiser events.");
            ViewBag.Error = "The RaceDay API is unavailable. Please try again later.";
            return View(new List<EventListItemViewModel>());
        }
    }

    [HttpGet]
    public IActionResult Create() => View(new EventFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            using var response = await _api.PostAsJsonAsync("api/events", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The event could not be saved. Check the details and try again.");
                return View(model);
            }

            TempData["Success"] = "Your event has been created.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not create event.");
            ModelState.AddModelError(string.Empty, "The RaceDay API is unavailable. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        try
        {
            using var response = await _api.GetAsync($"api/events/{Uri.EscapeDataString(id)}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var details = await response.Content.ReadFromJsonAsync<EventDetailsEnvelope>();
            if (details?.RaceEvent is null) return NotFound();

            var model = new EventFormViewModel
            {
                EventName = details.RaceEvent.EventName,
                EventType = details.RaceEvent.EventType,
                EventDate = details.RaceEvent.EventDate,
                Description = details.RaceEvent.Description,
                Location = details.RaceEvent.Location,
                DistanceKm = details.RaceEvent.DistanceKm,
                VenueName = details.Venue?.VenueName ?? details.RaceEvent.Location,
                City = details.Venue?.City ?? string.Empty,
                Province = details.Venue?.Province ?? string.Empty,
                BannerImageUrl = details.RaceEvent.BannerImageUrl
            };

            ViewBag.EventId = id;
            return View(model);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not load event for editing.");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EventFormViewModel model)
    {
        ViewBag.EventId = id;
        if (!ModelState.IsValid) return View(model);

        try
        {
            using var response = await _api.PutAsJsonAsync($"api/events/{Uri.EscapeDataString(id)}", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The event could not be updated. It may no longer be available.");
                return View(model);
            }

            TempData["Success"] = "Your event details have been updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not update event.");
            ModelState.AddModelError(string.Empty, "The RaceDay API is unavailable. Please try again later.");
            return View(model);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            using var response = await _api.DeleteAsync($"api/events/{Uri.EscapeDataString(id)}");
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "The event has been deleted.";
            else
                TempData["Error"] = "This event could not be deleted. It may already have participant entries.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not delete event.");
            TempData["Error"] = "The RaceDay API is unavailable. Please try again later.";
        }

        return RedirectToAction(nameof(Index));
    }
}
