using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TransactionStore.Application.Interfaces;
using TransactionStore.Core;

namespace TransactionStore.Application.Services;
public class CurrencyExchangeClient(
        ILogger<CurrencyExchangeClient> logger) 
    : ICurrencyExchangeClient
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, decimal>> _rates = new();

    public void UpdateRates(CurrencyType baseCurrency, Dictionary<string, decimal> rates)
    {
        logger.LogDebug($"Updating exchange rates for {baseCurrency} with {rates.Count} rates");
        var baseCurrencyKey = baseCurrency.ToString();
        var currencyRates = new ConcurrentDictionary<string, decimal>(rates);

        _rates[baseCurrencyKey] = currencyRates;
    }

    public Task<decimal> GetExchangeRateAsync(CurrencyType fromCurrency, CurrencyType toCurrency)
    {
        logger.LogInformation($"Getting exchange rate from {fromCurrency} to {toCurrency}");


        if (_rates.TryGetValue(fromCurrency.ToString(), out var fromRates) && fromRates.TryGetValue(toCurrency.ToString(), out var rate))
        {
            return Task.FromResult(rate);
        }

        logger.LogCritical($"Exchange rate from {fromCurrency} to {toCurrency} not found");
        throw new InvalidOperationException($"Exchange rate from {fromCurrency} to {toCurrency} not found");
    }

}