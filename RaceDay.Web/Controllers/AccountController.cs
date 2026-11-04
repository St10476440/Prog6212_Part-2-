using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

public sealed class AccountController(IHttpClientFactory clients, ILogger<AccountController> logger) : Controller
{
    private readonly HttpClient _api = clients.CreateClient("RaceDayApi");

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        try
        {
            using var response = await _api.PostAsJsonAsync("api/auth/login", new { model.Email, model.Password });
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "The email or password is incorrect."
                    : "We could not sign you in. Please try again.");
                return View(model);
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = json.RootElement;
            var token = root.GetProperty("token").GetString();
            var role = root.GetProperty("role").GetString();
            var userId = root.GetProperty("userId").GetString();
            var user = root.TryGetProperty("user", out var profile) ? profile : default;
            var fullName = user.ValueKind == JsonValueKind.Object && user.TryGetProperty("fullName", out var name)
                ? name.GetString() : model.Email;

            if (string.IsNullOrWhiteSpace(token) || (role != "Organiser" && role != "Participant"))
            {
                ModelState.AddModelError(string.Empty, "The API returned an invalid account response.");
                return View(model);
            }

            HttpContext.Session.SetString("AuthToken", token);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("UserId", userId ?? string.Empty);
            HttpContext.Session.SetString("FullName", fullName ?? model.Email);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", role == "Organiser" ? "Organiser" : "Participant");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "RaceDay API unavailable during login.");
            ModelState.AddModelError(string.Empty, "The RaceDay service is unavailable. Check that the API is running.");
            return View(model);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid login response from RaceDay API.");
            ModelState.AddModelError(string.Empty, "The service returned an unexpected response.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role is not ("Organiser" or "Participant"))
            ModelState.AddModelError(nameof(model.Role), "Choose Organiser or Participant.");

        if (!ModelState.IsValid) return View(model);

        try
        {
            using var response = await _api.PostAsJsonAsync("api/auth/register", new
            {
                model.FullName, model.Email, model.Password, model.Role, model.PhoneNumber
            });

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Registration could not be completed. Please check your details.");
                return View(model);
            }

            TempData["Success"] = "Your account has been created. Please sign in.";
            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "RaceDay API unavailable during registration.");
            ModelState.AddModelError(string.Empty, "The RaceDay service is unavailable. Please try again later.");
            return View(model);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
