using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionStore.Application.Consumers;
using TransactionStore.Application.Factories;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Messages;
using TransactionStore.Application.Services;

namespace TransactionStore.Application.Configurations;
public static class ApplicationConfiguration
{
    public static void ConfigureApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICurrencyExchangeClient, CurrencyExchangeClient>();
        services.AddScoped<ITransactionFactory, TransactionFactory>();
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]);
                    h.Password(configuration["RabbitMQ:Password"]);
                });

                cfg.ReceiveEndpoint("currency-exchange-queue", e =>
                {
                    e.Consumer<CurrencyExchangeRateConsumer>(context);
                });
                cfg.Message<TransactionCreated>(configTopology =>
                {
                    configTopology.SetEntityName("transactions.fanout");
                });
            });
        });
    }
}
