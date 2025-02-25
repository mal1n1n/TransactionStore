using MYPBackendMicroserviceIntegrations.Enums;

namespace TransactionStore.Application.Interfaces
{
    public interface ICurrencyExchangeClient
    {
        decimal GetExchangeRate(Currency fromCurrency, Currency toCurrency);
        void UpdateRates(Currency baseCurrency,Dictionary<string, decimal> rates);
    }
}