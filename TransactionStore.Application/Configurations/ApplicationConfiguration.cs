using MassTransit;
using MassTransit.Transports.Fabric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MYPBackendMicroserviceIntegrations.Messages;
using TransactionStore.Application.Consumers;
using TransactionStore.Application.Factories;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Services;
using TransactionStore.Application.Utils;

namespace TransactionStore.Application.Configurations;
public static class ApplicationConfiguration
{
    public static void ConfigureApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICommissionUtils, CommissionUtils>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICurrencyExchangeClient, CurrencyExchangeClient>();
        services.AddScoped<ITransactionFactory, TransactionFactory>();
        services.AddScoped<CurrencyExchangeRateConsumer>();
        services.AddOptions<RabbitMqConfiguration>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("RabbitMQ").Bind(options);
            });
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = context.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;

                cfg.Host(rabbitMqConfig.Host, h =>
                {
                    h.Username(rabbitMqConfig.Username);
                    h.Password(rabbitMqConfig.Password);
                });

                cfg.ReceiveEndpoint(rabbitMqConfig.CurrencyExchangeRateEndpoint, e =>
                {
                    e.Consumer<CurrencyExchangeRateConsumer>(context);
                });

                cfg.Publish<TransactionCreatedMessage>(e =>
                {
                    e.Durable = true;
                });
            });
        });
    }
}
