using Microsoft.Extensions.Options;
using System.Net;
using TransactionStore.Presentation.Models;

namespace TransactionStore.Presentation.Middlewares;
public class IpFilterMiddleware(
    RequestDelegate next, 
    IOptions<AllowedIpsOptions> allowedIPsOptions)
{
    private readonly AllowedIpsOptions _allowedIPsOptions = allowedIPsOptions.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null || !_allowedIPsOptions.AllowedIps.Contains(remoteIp.ToString()))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync($"Forbidden: Access is denied. {remoteIp}");
            return;
        }

        await next(context);
    }
}
