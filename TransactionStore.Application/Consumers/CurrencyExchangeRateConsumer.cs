using MassTransit;
using Microsoft.Extensions.Logging;
using TransactionStore.Application.Interfaces;
using TransactionStore.Core.DTOs;

namespace TransactionStore.Application.Consumers;

public class CurrencyExchangeRateConsumer(
        ILogger<CurrencyExchangeRateConsumer> logger, 
        ICurrencyExchangeClient currencyExchangeClient)
    : IConsumer<CurrencyRatesResponse>
{
    public async Task Consume(ConsumeContext<CurrencyRatesResponse> context)
    {
        logger.LogDebug($"Received exchange rate {context.Message.BaseCurrency.ToString()} from RabbitMQ: {context.Message.Rates}");
        currencyExchangeClient.UpdateRates(context.Message.Rates);
    }
}
