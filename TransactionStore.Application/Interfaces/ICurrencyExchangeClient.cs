using TransactionStore.Core;

namespace TransactionStore.Application.Interfaces
{
    public interface ICurrencyExchangeClient
    {
        Task<decimal> GetExchangeRateAsync(CurrencyType fromCurrency, CurrencyType toCurrency);
    }
}