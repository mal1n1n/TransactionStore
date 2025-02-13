using Microsoft.Extensions.Logging;
using Moq;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Services;
using TransactionStore.Core;
using Xunit;

namespace TransactionStore.Tests;

public class CurrencyExchangeServiceTests
{
    private readonly CurrencyExchangeClient _currencyExchangeClient;
    private readonly Mock<ILogger<CurrencyExchangeClient>> _loggerMock;

    public CurrencyExchangeServiceTests()
    {
        _loggerMock = new Mock<ILogger<CurrencyExchangeClient>>();
        _currencyExchangeClient = new CurrencyExchangeClient(_loggerMock.Object);
    }

    [Fact]
    public void UpdateRates_ShouldUpdateRatesCorrectly()
    {
        // Arrange
        var baseCurrency = CurrencyType.USD;
        var rates = new Dictionary<string, decimal>
        {
            { "EUR", 0.85m },
            { "JPY", 110m }
        };

        // Act
        _currencyExchangeClient.UpdateRates(baseCurrency, rates);

        // Assert
        Assert.Equal(0.85m, _currencyExchangeClient.GetExchangeRate(CurrencyType.USD, CurrencyType.EUR));
        Assert.Equal(1 / 0.85m, _currencyExchangeClient.GetExchangeRate(CurrencyType.EUR, CurrencyType.USD));
        Assert.Equal(110m, _currencyExchangeClient.GetExchangeRate(CurrencyType.USD, CurrencyType.JPY));
        Assert.Equal(1 / 110m, _currencyExchangeClient.GetExchangeRate(CurrencyType.JPY, CurrencyType.USD));
        Assert.Equal(110m / 0.85m, _currencyExchangeClient.GetExchangeRate(CurrencyType.EUR, CurrencyType.JPY));
        Assert.Equal(0.85m / 110m, _currencyExchangeClient.GetExchangeRate(CurrencyType.JPY, CurrencyType.EUR));
    }

    [Fact]
    public void GetExchangeRate_ShouldReturnCorrectRate()
    {
        // Arrange
        var baseCurrency = CurrencyType.USD;
        var rates = new Dictionary<string, decimal>
        {
            { "EUR", 0.85m },
            { "JPY", 110m }
        };
        _currencyExchangeClient.UpdateRates(baseCurrency, rates);

        // Act
        var rate = _currencyExchangeClient.GetExchangeRate(CurrencyType.USD, CurrencyType.EUR);

        // Assert
        Assert.Equal(0.85m, rate);
    }

    [Fact]
    public void GetExchangeRate_SameCurrency_ShouldReturnOne()
    {
        // Act
        var rate = _currencyExchangeClient.GetExchangeRate(CurrencyType.USD, CurrencyType.USD);

        // Assert
        Assert.Equal(1m, rate);
    }
}
