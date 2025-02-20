using MassTransit;
using Microsoft.Extensions.Logging;
using MYPBackendMicroserviceIntegrations.Messages;
using TransactionStore.Application.Interfaces;

namespace TransactionStore.Application.Consumers;

public class CurrencyExchangeRateConsumer(
        ILogger<CurrencyExchangeRateConsumer> logger, 
        ICurrencyExchangeClient currencyExchangeClient)
    : IConsumer<CurrencyRateMessage>
{
    public async Task Consume(ConsumeContext<CurrencyRateMessage> context)
    {
        logger.LogDebug($"Received exchange rate {context.Message.BaseCurrency.ToString()} from RabbitMQ: {context.Message.Rates}");
        currencyExchangeClient.UpdateRates(context.Message.Rates);
    }
}
