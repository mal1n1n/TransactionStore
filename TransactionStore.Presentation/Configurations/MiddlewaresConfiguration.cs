using TransactionStore.Presentation.Middlewares;

namespace TransactionStore.Presentation.Configurations;
public static class MiddlewaresConfiguration
{
    public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
    public static void UseIPFilter(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<IpFilterMiddleware>();
    }
}
