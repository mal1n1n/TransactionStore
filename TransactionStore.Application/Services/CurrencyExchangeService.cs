using System.Text.Json;
using TransactionStore.Application.Interfaces;
using TransactionStore.Core;
using TransactionStore.Core.DTOs;

namespace TransactionStore.Application.Services;
public class CurrencyExchangeClient : ICurrencyExchangeClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    public CurrencyExchangeClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<decimal> GetExchangeRateAsync(CurrencyType fromCurrency, CurrencyType toCurrency)
    {
        return 1.0m;
        var client = _httpClientFactory.CreateClient("CurrencyExchangeClient");
        var response = await client.GetAsync($"/rate?base={fromCurrency}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var exchangeRate = await JsonSerializer.DeserializeAsync<CurrencyRatesResponse>(stream);
        return exchangeRate.Rates[toCurrency.ToString()];
    }
}
