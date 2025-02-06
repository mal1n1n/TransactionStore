using Microsoft.Extensions.Logging;
using TransactionStore.Application.Interfaces;
using TransactionStore.Core;

namespace TransactionStore.Application.Services;
public class CurrencyExchangeClient(
        ILogger<CurrencyExchangeClient> logger) 
    : ICurrencyExchangeClient
{
    private readonly Dictionary<string, Dictionary<string, decimal>> _rates = [];

    public void UpdateRates(CurrencyType baseCurrency, Dictionary<string, decimal> rates)
    {
        logger.LogDebug($"Updating exchange rates for {baseCurrency} with {rates.Count} rates");
        var baseCurrencyKey = baseCurrency.ToString();
        var currencyRates = new Dictionary<string, decimal>(rates);

        _rates[baseCurrencyKey] = currencyRates;
    }

    public decimal GetExchangeRate(CurrencyType fromCurrency, CurrencyType toCurrency)
    {
        logger.LogInformation($"Getting exchange rate from {fromCurrency} to {toCurrency}");


        if (_rates.TryGetValue(fromCurrency.ToString(), out var fromRates) && fromRates.TryGetValue(toCurrency.ToString(), out var rate))
        {
            return rate;
        }

        logger.LogCritical($"Exchange rate from {fromCurrency} to {toCurrency} not found");
        throw new InvalidOperationException($"Exchange rate from {fromCurrency} to {toCurrency} not found");
    }

}