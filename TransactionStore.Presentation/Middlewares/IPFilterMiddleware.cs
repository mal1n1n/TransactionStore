using System.Net;

namespace TransactionStore.Presentation.Middlewares;
public class IPFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _allowedIP;

    public IPFilterMiddleware(RequestDelegate next, string allowedIP)
    {
        _next = next;
        _allowedIP = allowedIP;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null || remoteIp.ToString() != _allowedIP)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Forbidden: Access is denied.");
            return;
        }

        await _next(context);
    }
}
