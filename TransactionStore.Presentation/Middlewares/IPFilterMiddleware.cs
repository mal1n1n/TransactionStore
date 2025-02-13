using System.Net;

namespace TransactionStore.Presentation.Middlewares;
public class IPFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _allowedIPs;

    public IPFilterMiddleware(RequestDelegate next, string[] allowedIPs)
    {
        _next = next;
        _allowedIPs = allowedIPs;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null || !_allowedIPs.Contains(remoteIp.ToString()))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync($"Forbidden: Access is denied. {remoteIp}");
            return;
        }

        await _next(context);
    }
}
