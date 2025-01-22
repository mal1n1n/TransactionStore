using TransactionStore.Application.Interfaces;
using TransactionStore.Core;

namespace TransactionStore.Application.Services;
public class CurrencyExchangeClient : ICurrencyExchangeClient
{
    public async Task<decimal> GetExchangeRateAsync(CurrencyType fromCurrency, CurrencyType toCurrency)
    {
        var randResult = new Random().Next(1, 100);
        return await Task.FromResult((decimal)randResult);
    }
}
