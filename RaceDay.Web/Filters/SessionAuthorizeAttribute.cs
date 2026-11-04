using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute(params string[] allowedRoles) : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var role = session.GetString("UserRole");

        if (string.IsNullOrWhiteSpace(session.GetString("AuthToken")))
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
        }
        else if (allowedRoles.Length > 0 && (role is null || !allowedRoles.Contains(role, StringComparer.Ordinal)))
        {
            context.Result = new RedirectToActionResult("Index", role == "Organiser" ? "Organiser" : "Participant", null);
        }

        return Task.CompletedTask;
    }
}
