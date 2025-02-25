using MYPBackendMicroserviceIntegrations.Enums;
using TransactionStore.Core;

namespace TransactionStore.Application.Interfaces
{
    public interface ICurrencyExchangeClient
    {
        decimal GetExchangeRate(CurrencyType fromCurrency, CurrencyType toCurrency);
        void UpdateRates(Currency baseCurrency,Dictionary<string, decimal> rates);
    }
}