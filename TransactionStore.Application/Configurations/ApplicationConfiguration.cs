using Microsoft.Extensions.DependencyInjection;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Services;

namespace TransactionStore.Application.Configurations;
public static class ApplicationConfiguration
{
    public static void ConfigureApplication(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICurrencyExchangeClient, CurrencyExchangeClient>();
    }
}
