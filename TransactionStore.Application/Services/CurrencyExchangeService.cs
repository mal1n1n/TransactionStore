using Microsoft.Extensions.Logging;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Core;

namespace TransactionStore.Application.Services;
public class CurrencyExchangeClient(
        ILogger<CurrencyExchangeClient> logger) 
    : ICurrencyExchangeClient
{
    private readonly Dictionary<string, decimal> _rates = new();

    public void UpdateRates(CurrencyType baseCurrency, Dictionary<string, decimal> rates)
    {
        logger.LogDebug($"Updating exchange rates for {baseCurrency} with {rates.Count} rates");
        _rates.Clear();

        foreach (var rate in rates)
        {
            _rates[$"{baseCurrency}{rate.Key}"] = rate.Value;
            _rates[$"{rate.Key}{baseCurrency}"] = 1 / rate.Value;
        }

        foreach (var fromRate in rates)
        {
            foreach (var toRate in rates)
            {
                if (fromRate.Key != toRate.Key)
                {
                    _rates[$"{fromRate.Key}{toRate.Key}"] = toRate.Value / fromRate.Value;
                }
            }
        }
    }


    public decimal GetExchangeRate(CurrencyType fromCurrency, CurrencyType toCurrency)
    {
        logger.LogInformation($"Getting exchange rate from {fromCurrency} to {toCurrency}");

        if (fromCurrency == toCurrency)
        {
            logger.LogWarning($"Exchange rate client trying to get rates for same currencies");
            return 1m;
        }

        var key = $"{fromCurrency}{toCurrency}";
        if (_rates.TryGetValue(key, out var rate))
        {
            return rate;
        }

        logger.LogCritical($"Exchange rate from {fromCurrency} to {toCurrency} not found");
        throw new ExchangeRateNotFoundException($"Exchange rate from {fromCurrency} to {toCurrency} not found");
    }

}