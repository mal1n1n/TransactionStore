using Microsoft.Extensions.Logging;
using Moq;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
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

    [Fact]
    public void UpdateRates_ShouldUpdateRatesCorrectly()
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
        Assert.Equal(0.85m, _sut.GetExchangeRate(CurrencyType.USD, CurrencyType.EUR));
        Assert.Equal(1.18m, _sut.GetExchangeRate(CurrencyType.EUR, CurrencyType.USD));
        Assert.Equal(110m, _sut.GetExchangeRate(CurrencyType.USD, CurrencyType.JPY));
        Assert.Equal(0.0077m, _sut.GetExchangeRate(CurrencyType.JPY, CurrencyType.EUR));
    }

    [Fact]
    public void GetExchangeRate_ShouldReturnCorrectRate()
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.85m },
            { "USDJPY", 110m }
        };
        _sut.UpdateRates(rates);

        // Act
        var rate = _sut.GetExchangeRate(CurrencyType.USD, CurrencyType.EUR);

        // Assert
        Assert.Equal(0.85m, rate);
    }

    [Fact]
    public void GetExchangeRate_SameCurrency_ShouldReturnOne()
    {
        // Act
        var rate = _sut.GetExchangeRate(CurrencyType.USD, CurrencyType.USD);

        // Assert
        Assert.Equal(1m, rate);
    }

    [Fact]
    public void GetExchangeRate_InvalidCurrency_ShouldThrowExchangeRateNotFoundException()
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "USDEUR", 0.85m },
            { "USDJPY", 110m }
        };
        _sut.UpdateRates(rates);

        var fromCurrency = CurrencyType.USD;
        var toCurrency = CurrencyType.ARS;

        // Act & Assert
        Assert.Throws<ExchangeRateNotFoundException>(() => _sut.GetExchangeRate(fromCurrency, toCurrency));
    }
}
