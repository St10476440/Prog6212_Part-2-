using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Participant")]
public sealed class ParticipantController(
    IHttpClientFactory clients,
    ILogger<ParticipantController> logger) : Controller
{
    private readonly HttpClient _api = clients.CreateClient("RaceDayApi");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new ParticipantDashboardViewModel();

        try
        {
            using var eventsResponse = await _api.GetAsync("api/events");
            if (eventsResponse.IsSuccessStatusCode)
            {
                model.Events = await eventsResponse.Content.ReadFromJsonAsync<List<EventListItemViewModel>>()
                    ?? new List<EventListItemViewModel>();
            }
            else
            {
                model.ErrorMessage = "Events could not be loaded.";
            }

            using var entriesResponse = await _api.GetAsync("api/enrolments/my-entries");
            if (entriesResponse.IsSuccessStatusCode)
            {
                model.Entries = await entriesResponse.Content.ReadFromJsonAsync<List<ParticipantEntryViewModel>>()
                    ?? new List<ParticipantEntryViewModel>();
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not load participant dashboard.");
            model.ErrorMessage = "The RaceDay API is unavailable. Please try again later.";
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        try
        {
            using var response = await _api.GetAsync($"api/events/{Uri.EscapeDataString(id)}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var model = await response.Content.ReadFromJsonAsync<ParticipantEventDetailsViewModel>();
            if (model?.RaceEvent is null) return NotFound();

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not load participant event details.");
            TempData["Error"] = "The RaceDay API is unavailable.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(string eventId, string categoryId)
    {
        try
        {
            using var response = await _api.PostAsJsonAsync("api/enrolments", new
            {
                EventID = eventId,
                CategoryID = categoryId
            });

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Your entry has been submitted.";
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                TempData["Error"] = "You already have an entry for this event.";
            else
                TempData["Error"] = "Your entry could not be submitted. Please check the category and try again.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not submit participant enrolment.");
            TempData["Error"] = "The RaceDay API is unavailable. Please try again later.";
        }

        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    [HttpGet]
    public async Task<IActionResult> MyHistory()
    {
        try
        {
            using var response = await _api.GetAsync("api/results/my-history");
            if (!response.IsSuccessStatusCode)
                return View(new List<ParticipantResultViewModel>());

            var results = await response.Content.ReadFromJsonAsync<List<ParticipantResultViewModel>>()
                ?? new List<ParticipantResultViewModel>();
            return View(results);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not load participant results history.");
            ViewBag.Error = "The RaceDay API is unavailable.";
            return View(new List<ParticipantResultViewModel>());
        }
    }
}
