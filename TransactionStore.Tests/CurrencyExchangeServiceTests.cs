using Microsoft.Extensions.Logging;
using Moq;
using MYPBackendMicroserviceIntegrations.Enums;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Services;
using Xunit;

namespace TransactionStore.Tests;

public class CurrencyExchangeServiceTests
{
    private readonly CurrencyExchangeClient _sut;
    private readonly Mock<ILogger<CurrencyExchangeClient>> _loggerMock;

    public CurrencyExchangeServiceTests()
    {
        _loggerMock = new Mock<ILogger<CurrencyExchangeClient>>();
        _sut = new CurrencyExchangeClient(_loggerMock.Object);
    }

    [Theory]
    [InlineData("USDEUR", 0.85)]
    [InlineData("USDJPY", 110)]
    [InlineData("EURUSD", 1.18)]
    [InlineData("JPYEUR", 0.0077)]
    public void UpdateRates_ShouldUpdateRatesCorrectly(string currencyPair, decimal expectedRate)
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.85m },
            { "USDJPY", 110m },
            { "EURUSD", 1.18m },
            { "JPYEUR", 0.0077m }
        };

        // Act
        _sut.UpdateRates(Currency.USD, rates);

        // Assert
        var fromCurrency = (Currency)Enum.Parse(typeof(Currency), currencyPair.Substring(0, 3));
        var toCurrency = (Currency)Enum.Parse(typeof(Currency), currencyPair.Substring(3, 3));
        var actualRate = _sut.GetExchangeRate(fromCurrency, toCurrency);
        Assert.Equal(expectedRate, actualRate, 4);
    }

    [Theory]
    [InlineData(Currency.USD, Currency.EUR, 0.85)]
    [InlineData(Currency.USD, Currency.JPY, 110)]
    public void GetExchangeRate_ShouldReturnCorrectRate(Currency fromCurrency, Currency toCurrency, decimal expectedRate)
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.85m },
            { "USDJPY", 110m }
        };
        _sut.UpdateRates(Currency.USD, rates);

        // Act
        var rate = _sut.GetExchangeRate(fromCurrency, toCurrency);

        // Assert
        Assert.Equal(expectedRate, rate);
    }

    [Theory]
    [InlineData(Currency.USD)]
    [InlineData(Currency.EUR)]
    public void GetExchangeRate_SameCurrency_ShouldReturnOne(Currency currency)
    {
        // Act
        var rate = _sut.GetExchangeRate(currency, currency);

        // Assert
        Assert.Equal(1m, rate);
    }

    [Theory]
    [InlineData(Currency.USD, Currency.ARS)]
    [InlineData(Currency.EUR, Currency.RUB)]
    public void GetExchangeRate_InvalidCurrency_ShouldThrowExchangeRateNotFoundException(Currency fromCurrency, Currency toCurrency)
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.85m },
            { "USDJPY", 110m }
        };
        _sut.UpdateRates(Currency.USD, rates);

        // Act & Assert
        Assert.Throws<ExchangeRateNotFoundException>(() => _sut.GetExchangeRate(fromCurrency, toCurrency));
    }
}
