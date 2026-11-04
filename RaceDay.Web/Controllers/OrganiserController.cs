using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize("Organiser")]
public sealed class OrganiserController(
    IHttpClientFactory clients,
    AzureBlobStorageService blobStorage,
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
            if (model.BannerImage is { Length: > 0 })
                model.BannerImageUrl = await blobStorage.UploadEventBannerAsync(model.BannerImage);

            using var response = await _api.PostAsJsonAsync("api/events", new
            {
                model.EventName, model.EventType, model.EventDate, model.Description,
                model.Location, model.DistanceKm, model.VenueName, model.City,
                model.Province, model.BannerImageUrl
            });
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The event could not be saved. Check the details and try again.");
                return View(model);
            }

            TempData["Success"] = "Your event has been created.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.BannerImage), ex.Message);
            return View(model);
        }
        catch (Azure.RequestFailedException ex)
        {
            logger.LogError(ex, "Azure Blob Storage rejected the event banner upload.");
            ModelState.AddModelError(nameof(model.BannerImage), "The banner could not be uploaded. Check the Azure Storage configuration and try again.");
            return View(model);
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
            if (model.BannerImage is { Length: > 0 })
                model.BannerImageUrl = await blobStorage.UploadEventBannerAsync(model.BannerImage);

            using var response = await _api.PutAsJsonAsync($"api/events/{Uri.EscapeDataString(id)}", new
            {
                model.EventName, model.EventType, model.EventDate, model.Description,
                model.Location, model.DistanceKm, model.VenueName, model.City,
                model.Province, model.BannerImageUrl
            });
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The event could not be updated. It may no longer be available.");
                return View(model);
            }

            TempData["Success"] = "Your event details have been updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.BannerImage), ex.Message);
            return View(model);
        }
        catch (Azure.RequestFailedException ex)
        {
            logger.LogError(ex, "Azure Blob Storage rejected the event banner update.");
            ModelState.AddModelError(nameof(model.BannerImage), "The banner could not be uploaded. Check the Azure Storage configuration and try again.");
            return View(model);
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

    [HttpGet]
    public async Task<IActionResult> ManageCategories(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        try
        {
            using var response = await _api.GetAsync($"api/events/{Uri.EscapeDataString(id)}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var details = await response.Content.ReadFromJsonAsync<EventDetailsEnvelope>();
            if (details?.RaceEvent is null) return NotFound();

            return View(new CategoryManagementViewModel
            {
                EventID = details.RaceEvent.EventID,
                EventName = details.RaceEvent.EventName,
                Categories = details.Categories
            });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not load event categories.");
            TempData["Error"] = "The RaceDay API is unavailable.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(string id, CategoryFormViewModel model)
    {
        if (model.MinimumAge.HasValue && model.MaximumAge.HasValue
            && model.MinimumAge > model.MaximumAge)
            ModelState.AddModelError(nameof(model.MaximumAge), "Maximum age must be greater than or equal to minimum age.");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the category details and try again.";
            return RedirectToAction(nameof(ManageCategories), new { id });
        }

        try
        {
            using var response = await _api.PostAsJsonAsync($"api/events/{Uri.EscapeDataString(id)}/categories", model);
            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
                ? "The category has been added."
                : "The category could not be added. Check the event and category details.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not add event category.");
            TempData["Error"] = "The RaceDay API is unavailable.";
        }

        return RedirectToAction(nameof(ManageCategories), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(string id, string categoryId)
    {
        try
        {
            using var response = await _api.DeleteAsync(
                $"api/events/{Uri.EscapeDataString(id)}/categories/{Uri.EscapeDataString(categoryId)}");
            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
                ? "The category has been removed."
                : "This category cannot be removed because it may already have participant entries.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not remove event category.");
            TempData["Error"] = "The RaceDay API is unavailable.";
        }

        return RedirectToAction(nameof(ManageCategories), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ManageEntries(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        try
        {
            using var eventResponse = await _api.GetAsync($"api/events/{Uri.EscapeDataString(id)}");
            using var entriesResponse = await _api.GetAsync($"api/events/{Uri.EscapeDataString(id)}/entries");
            if (!eventResponse.IsSuccessStatusCode || !entriesResponse.IsSuccessStatusCode)
                return NotFound();

            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsEnvelope>();
            if (eventDetails?.RaceEvent is null) return NotFound();

            var entries = await entriesResponse.Content.ReadFromJsonAsync<List<EventEntryViewModel>>()
                ?? new List<EventEntryViewModel>();

            return View(new EventEntriesPageViewModel
            {
                EventID = id,
                EventName = eventDetails.RaceEvent.EventName,
                Entries = entries
            });
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not load event participant entries.");
            TempData["Error"] = "The RaceDay API is unavailable.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResult(string id, ResultFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide a finish time, position and result status.";
            return RedirectToAction(nameof(ManageEntries), new { id });
        }

        try
        {
            using var response = await _api.PostAsJsonAsync(
                $"api/events/{Uri.EscapeDataString(id)}/results", model);
            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
                ? "The participant result has been recorded."
                : "The result could not be saved. Check that this entry does not already have a result.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not record participant result.");
            TempData["Error"] = "The RaceDay API is unavailable.";
        }

        return RedirectToAction(nameof(ManageEntries), new { id });
    }
}
