using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Factories;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;
using Xunit;

namespace TransactionStore.Tests;

public class TransactionFactoryTests
{
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<TransactionFactory>> _logger;
    private readonly Mock<IConfiguration> _configuration;
    private readonly TransactionFactory _sut;

    public TransactionFactoryTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TransactionModel, Transaction>().ReverseMap();
        });
        _mapper = new Mapper(configuration);
        _logger = new Mock<ILogger<TransactionFactory>>();
        _configuration = new Mock<IConfiguration>();
        _sut = new TransactionFactory(_mapper, _logger.Object, _configuration.Object);
    }

    [Theory]
    [InlineData(TransactionType.Deposit, 100, 1.5, 1.50)]
    [InlineData(TransactionType.Withdrawal, 100, 2.1, 2.10)]
    [InlineData(TransactionType.Transfer, 100, 3.4, 3.40)]
    public void CreateTransaction_CorrectlyCalculatesCommission(TransactionType transactionType, decimal amount, decimal commissionRate, decimal expectedCommission)
    {
        // Arrange
        var transactionModel = new TransactionModel
        {
            AccountId = Guid.NewGuid(),
            Amount = amount,
            Date = DateTime.UtcNow,
            TransactionType = transactionType
        };

        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(a => a.Value).Returns(commissionRate.ToString(CultureInfo.InvariantCulture));
        _configuration.Setup(config => config.GetSection($"TransactionCommissionRates:{transactionType}")).Returns(sectionMock.Object);

        // Act
        var transaction = _sut.CreateTransaction(transactionModel, transactionType, out var commission);

        // Assert
        commission.Should().Be(expectedCommission);
        if (transactionType == TransactionType.Withdrawal)
        {
            transaction.Amount.Should().Be(-(amount - expectedCommission));
        }
        else
        {
            transaction.Amount.Should().Be(amount - expectedCommission);
        }
    }


    [Theory]
    [InlineData(100, 3.0, 0.85, 3.0)]
    public void CreateTransferTransaction_CorrectlyCalculatesCommission(decimal amount, decimal commissionRate, decimal exchangeRate, decimal expectedCommission)
    {
        // Arrange
        var transferModel = new TransferTransactionModel
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = amount,
            FromCurrency = CurrencyType.USD,
            ToCurrency = CurrencyType.EUR
        };

        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(a => a.Value).Returns(commissionRate.ToString());
        _configuration.Setup(config => config.GetSection("TransactionCommissionRates:Transfer")).Returns(sectionMock.Object);

        // Act
        var (transFrom, transTo, commission) = _sut.CreateTransferTransaction(transferModel, exchangeRate);

        // Assert
        commission.Should().Be(expectedCommission);
        transFrom.Amount.Should().Be(-(amount - expectedCommission));
        transTo.Amount.Should().Be((amount - expectedCommission) * exchangeRate);
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Withdrawal)]
    [InlineData(TransactionType.Transfer)]
    public void CreateTransaction_MissingCommissionRate_ThrowsCommissionSettingException(TransactionType transactionType)
    {
        // Arrange
        var transactionModel = new TransactionModel
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Date = DateTime.UtcNow,
            TransactionType = transactionType
        };

        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(a => a.Value).Returns((string)null);
        _configuration.Setup(config => config.GetSection($"TransactionCommissionRates:{transactionType}")).Returns(sectionMock.Object);

        // Act
        Action act = () => _sut.CreateTransaction(transactionModel, transactionType, out _);

        // Assert
        act.Should().Throw<CommissionSettingException>()
            .WithMessage($"TransactionCommissionRates:{transactionType} is not configured or went broken.");
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Withdrawal)]
    [InlineData(TransactionType.Transfer)]
    public void CreateTransaction_NegativeCommissionRate_ThrowsCommissionSettingException(TransactionType transactionType)
    {
        // Arrange
        var transactionModel = new TransactionModel
        {
            AccountId = Guid.NewGuid(),
            Amount = 100,
            Date = DateTime.UtcNow,
            TransactionType = transactionType
        };

        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(a => a.Value).Returns("-1");
        _configuration.Setup(config => config.GetSection($"TransactionCommissionRates:{transactionType}")).Returns(sectionMock.Object);

        // Act
        Action act = () => _sut.CreateTransaction(transactionModel, transactionType, out _);

        // Assert
        act.Should().Throw<CommissionSettingException>()
            .WithMessage($"TransactionCommissionRates:{transactionType} is not configured or went broken.");
    }

    [Fact]
    public void CreateTransferTransaction_MissingCommissionRate_ThrowsCommissionSettingException()
    {
        // Arrange
        var transferModel = new TransferTransactionModel
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 100,
            FromCurrency = CurrencyType.USD,
            ToCurrency = CurrencyType.EUR
        };

        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(a => a.Value).Returns((string)null);
        _configuration.Setup(config => config.GetSection("TransactionCommissionRates:Transfer")).Returns(sectionMock.Object);

        // Act
        Action act = () => _sut.CreateTransferTransaction(transferModel, 0.85m);

        // Assert
        act.Should().Throw<CommissionSettingException>()
            .WithMessage("TransactionCommissionRates:Transfer is not configured or went broken.");
    }

    [Fact]
    public void CreateTransferTransaction_NegativeCommissionRate_ThrowsCommissionSettingException()
    {
        // Arrange
        var transferModel = new TransferTransactionModel
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 100,
            FromCurrency = CurrencyType.USD,
            ToCurrency = CurrencyType.EUR
        };

        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(a => a.Value).Returns("-1");
        _configuration.Setup(config => config.GetSection("TransactionCommissionRates:Transfer")).Returns(sectionMock.Object);

        // Act
        Action act = () => _sut.CreateTransferTransaction(transferModel, 0.85m);

        // Assert
        act.Should().Throw<CommissionSettingException>()
            .WithMessage("TransactionCommissionRates:Transfer is not configured or went broken.");
    }
}