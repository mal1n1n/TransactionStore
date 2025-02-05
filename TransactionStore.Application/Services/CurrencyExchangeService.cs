using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionStore.Application.Interfaces;
using TransactionStore.Core;
using TransactionStore.Core.DTOs;

namespace TransactionStore.Application.Services;
public class CurrencyExchangeClient(
        ILogger<CurrencyExchangeClient> logger
    ) : ICurrencyExchangeClient
{
    public async Task<decimal> GetExchangeRateAsync(CurrencyType fromCurrency, CurrencyType toCurrency)
    {
        logger.LogInformation($"Getting exchange rate from {fromCurrency} to {toCurrency}");
        return 1.0m;
        //var exchangeRate = await JsonSerializer.DeserializeAsync<CurrencyRatesResponse>(stream);
        //return exchangeRate.Rates[toCurrency.ToString()];
    }
}
