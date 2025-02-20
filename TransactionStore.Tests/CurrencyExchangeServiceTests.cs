using Microsoft.Extensions.Logging;
using Moq;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Services;
using TransactionStore.Core;
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
        _sut.UpdateRates(rates);

        // Assert
        var fromCurrency = (CurrencyType)Enum.Parse(typeof(CurrencyType), currencyPair.Substring(0, 3));
        var toCurrency = (CurrencyType)Enum.Parse(typeof(CurrencyType), currencyPair.Substring(3, 3));
        Assert.Equal(expectedRate, _sut.GetExchangeRate(fromCurrency, toCurrency));
    }

    [Theory]
    [InlineData(CurrencyType.USD, CurrencyType.EUR, 0.85)]
    [InlineData(CurrencyType.USD, CurrencyType.JPY, 110)]
    public void GetExchangeRate_ShouldReturnCorrectRate(CurrencyType fromCurrency, CurrencyType toCurrency, decimal expectedRate)
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.85m },
            { "USDJPY", 110m }
        };
        _sut.UpdateRates(rates);

        // Act
        var rate = _sut.GetExchangeRate(fromCurrency, toCurrency);

        // Assert
        Assert.Equal(expectedRate, rate);
    }

    [Theory]
    [InlineData(CurrencyType.USD)]
    [InlineData(CurrencyType.EUR)]
    public void GetExchangeRate_SameCurrency_ShouldReturnOne(CurrencyType currency)
    {
        // Act
        var rate = _sut.GetExchangeRate(currency, currency);

        // Assert
        Assert.Equal(1m, rate);
    }

    [Theory]
    [InlineData(CurrencyType.USD, CurrencyType.ARS)]
    [InlineData(CurrencyType.EUR, CurrencyType.RUB)]
    public void GetExchangeRate_InvalidCurrency_ShouldThrowExchangeRateNotFoundException(CurrencyType fromCurrency, CurrencyType toCurrency)
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.85m },
            { "USDJPY", 110m }
        };
        _sut.UpdateRates(rates);

        // Act & Assert
        Assert.Throws<ExchangeRateNotFoundException>(() => _sut.GetExchangeRate(fromCurrency, toCurrency));
    }
}
