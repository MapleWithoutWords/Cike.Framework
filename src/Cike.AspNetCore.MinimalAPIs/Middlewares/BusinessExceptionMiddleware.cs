using Cike.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cike.AspNetCore.MinimalAPIs.Middlewares;

public class BusinessExceptionMiddleware
{
    private RequestDelegate _next;

    private readonly ILogger<BusinessExceptionMiddleware> _logger;

    public BusinessExceptionMiddleware(RequestDelegate next, ILogger<BusinessExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            _logger?.Log(ex.LogLevel, ex, ex.Message);
            await Results.BadRequest(ex.Message).ExecuteAsync(context);
        }
    }
}
