using Microsoft.Extensions.Logging;
using MYPBackendMicroserviceIntegrations.Enums;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;

namespace TransactionStore.Application.Services;
public class CurrencyExchangeClient(
        ILogger<CurrencyExchangeClient> logger) 
    : ICurrencyExchangeClient
{
    private static readonly Dictionary<string, decimal> _rates = new();

    public void UpdateRates(Currency baseCurrency, Dictionary<string, decimal> rates)
    {
        logger.LogDebug($"Updating exchange rates with {rates.Count} rates");
        _rates.Clear();
        foreach (var rate in rates)
        {
            _rates[rate.Key] = rate.Value;
        }

        foreach (var fromCurrency in Enum.GetValues<Currency>())
        {
            foreach (var toCurrency in Enum.GetValues<Currency>())
            {
                if (fromCurrency == toCurrency || fromCurrency == Currency.Unknown || toCurrency == Currency.Unknown)
                    continue;

                var fromBaseKey = $"{baseCurrency}{fromCurrency}";
                var toBaseKey = $"{baseCurrency}{toCurrency}";

                if (_rates.TryGetValue(fromBaseKey, out var fromBaseRate) && _rates.TryGetValue(toBaseKey, out var toBaseRate))
                {
                    var crossRate = toBaseRate / fromBaseRate;
                    var crossKey = $"{fromCurrency}{toCurrency}";
                    _rates[crossKey] = crossRate;
                }
            }
        }
    }

    public decimal GetExchangeRate(Currency fromCurrency, Currency toCurrency)
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