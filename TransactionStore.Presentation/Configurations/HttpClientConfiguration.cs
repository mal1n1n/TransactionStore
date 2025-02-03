namespace TransactionStore.Presentation.Configurations;
public static class HttpClientConfiguration
{
    public static void ConfigureHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("CurrencyExchangeClient", client =>
        {
            // exchange rates api
            client.BaseAddress = new Uri("https://api.exchangeratesapi.io");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
    }
}
