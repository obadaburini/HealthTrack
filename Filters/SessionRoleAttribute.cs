using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelthTrack.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionRoleAttribute(string role) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Session.GetString("UserRole") != role ||
            string.IsNullOrWhiteSpace(context.HttpContext.Session.GetString("UserName")))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}
