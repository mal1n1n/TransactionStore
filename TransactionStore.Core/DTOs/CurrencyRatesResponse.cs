namespace TransactionStore.Core.DTOs;
public class CurrencyRatesResponse
{
    public CurrencyType BaseCurrency { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
    public DateTime RatesDate { get; set; }
}
