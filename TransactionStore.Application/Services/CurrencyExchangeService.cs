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

    public void UpdateRates(Dictionary<string, decimal> rates)
    {
        logger.LogDebug($"Updating exchange rates with {rates.Count} rates");
        _rates.Clear();

        foreach (var rate in rates)
        {
            _rates[rate.Key] = rate.Value;
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
            if (rate <= 0)
            {
                logger.LogCritical("Invalid exchange rate for CurrencyPair: {CurrencyPair}", key);
                throw new InvalidExchangeRateException("Invalid exchange rate");
            }
            return rate;
        }

        logger.LogCritical($"Exchange rate from {fromCurrency} to {toCurrency} not found");
        throw new ExchangeRateNotFoundException($"Exchange rate from {fromCurrency} to {toCurrency} not found");
    }

}