using Microsoft.Extensions.Configuration;

namespace TransactionStore.Application.Configurations;

public class RabbitMqConfiguration
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string CurrencyExchangeRateEndpoint { get; set; }
}
