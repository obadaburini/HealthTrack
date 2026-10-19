using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace HelthTrack.Filters;

public sealed class DatabaseExceptionFilter(ILogger<DatabaseExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not (DbException or DbUpdateException)) return;
        // Database exceptions can contain connection details and personal data.
        logger.LogError("Database operation failed ({ExceptionType}); check database connectivity and migrations.", context.Exception.GetType().Name);
        context.Result = new ViewResult { ViewName = "ServiceUnavailable", StatusCode = 503 };
        context.ExceptionHandled = true;
    }
}
