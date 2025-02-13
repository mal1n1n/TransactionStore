using TransactionStore.Core;

namespace TransactionStore.Application.Interfaces
{
    public interface ICurrencyExchangeClient
    {
        decimal GetExchangeRate(CurrencyType fromCurrency, CurrencyType toCurrency);
        void UpdateRates(Dictionary<string, decimal> rates);
    }
}