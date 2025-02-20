using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Factories;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Application.Services;
using TransactionStore.Application.Utils;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;
using Xunit;

namespace TransactionStore.Tests;

public class TransactionFactoryTests
{
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<TransactionFactory>> _logger;
    private readonly Mock<IConfiguration> _configuration;
    private readonly Mock<ICurrencyExchangeClient> _currencyExchangeClient;
    private readonly Mock<ICommissionUtils> _commissionUtils;
    private readonly TransactionFactory _sut;

    public TransactionFactoryTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TransactionModel, Transaction>().ReverseMap();
        });
        _mapper = new Mapper(configuration);
        _logger = new();
        _currencyExchangeClient = new();
        _commissionUtils = new();
        _configuration = new();
        _sut = new TransactionFactory(
            _mapper, 
            _logger.Object, 
            _configuration.Object, 
            _currencyExchangeClient.Object,
            _commissionUtils.Object);
    }

    [Theory]
    [InlineData(TransactionType.Deposit, 100, 1.50)]
    [InlineData(TransactionType.Withdrawal, 100, 2.10)]
    public void CreateTransaction_CorrectlyCalculatesCommission(TransactionType transactionType, decimal amount, decimal expectedCommission)
    {
        // Arrange
        var transactionModel = new TransactionModel
        {
            AccountId = Guid.NewGuid(),
            Amount = amount,
            Date = DateTime.UtcNow,
            TransactionType = transactionType
        };

        _commissionUtils.Setup(cu => cu.CalculateCommission(amount, transactionType)).Returns(expectedCommission);

        // Act
        var transaction = _sut.CreateTransaction(transactionModel, transactionType);

        // Assert
        if (transactionType == TransactionType.Withdrawal)
        {
            transaction.Amount.Should().Be(-amount);
        }
        else
        {
            transaction.Amount.Should().Be(amount - expectedCommission);
        }
    }


    [Theory]
    [InlineData(100, 0.85, 3.0)]
    public void CreateTransferTransaction_CorrectlyCalculatesCommission(decimal amount, decimal exchangeRate, decimal expectedCommission)
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

        _commissionUtils.Setup(cu => cu.CalculateCommission(amount, TransactionType.Transfer)).Returns(expectedCommission);
        _currencyExchangeClient.Setup(ce => ce.GetExchangeRate(transferModel.FromCurrency, transferModel.ToCurrency)).Returns(exchangeRate);

        // Act
        var result = _sut.CreateTransferTransactions(transferModel);

        // Assert
        result.TransFrom.Amount.Should().Be(-amount);
        result.TransTo.Amount.Should().Be((amount - expectedCommission) * exchangeRate);
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Withdrawal)]
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

        _commissionUtils.Setup(cu => cu.CalculateCommission(It.IsAny<decimal>(), transactionType)).Throws(new CommissionSettingException($"TransactionCommissionRates:{transactionType} is not configured or went broken."));

        // Act
        Action act = () => _sut.CreateTransaction(transactionModel, transactionType);

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

        _commissionUtils.Setup(cu => cu.CalculateCommission(It.IsAny<decimal>(), TransactionType.Transfer)).Throws(new CommissionSettingException("TransactionCommissionRates:Transfer is not configured or went broken."));

        // Act
        Action act = () => _sut.CreateTransferTransactions(transferModel);

        // Assert
        act.Should().Throw<CommissionSettingException>()
            .WithMessage("TransactionCommissionRates:Transfer is not configured or went broken.");
    }
}