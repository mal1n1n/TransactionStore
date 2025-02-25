using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MYPBackendMicroserviceIntegrations.Enums;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Presentation.Controllers;
using TransactionStore.Presentation.DTOs.Requests;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Tests;

public class TransactionControllerTests
{
    private readonly Mock<ILogger<TransactionController>> _loggerMock;
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly IMapper mapper;
    private readonly TransactionController _sut;

    public TransactionControllerTests()
    {
        _loggerMock = new Mock<ILogger<TransactionController>>();
        _transactionServiceMock = new Mock<ITransactionService>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CreateTransactionRequest, TransactionModel>();
            cfg.CreateMap<CreateTransferRequest, TransferTransactionModel>();
            cfg.CreateMap<TransactionModel, TransactionResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TransactionType));
        });
        mapper = config.CreateMapper();
        _sut = new TransactionController(_loggerMock.Object, _transactionServiceMock.Object, mapper);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsTransactionResponse()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transactionModel = new TransactionModel
        {
            Id = transactionId,
            AccountId = Guid.NewGuid(),
            Amount = 100,
            TransactionType = TransactionType.Deposit,
            Date = DateTime.UtcNow
        };
        var transactionResponse = mapper.Map<TransactionResponse>(transactionModel);

        _transactionServiceMock.Setup(s => s.GetTransactionByIdAsync(transactionId))
            .ReturnsAsync(transactionModel);

        // Act
        var result = await _sut.GetTransactionById(transactionId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var response = okResult.Value as TransactionResponse;
        response.Should().BeEquivalentTo(transactionResponse);
    }

    [Fact]
    public async Task CreateDepositTransaction_ReturnsGuid()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            Currency = Currency.USD,
            Amount = 100
        };
        var transactionModel = mapper.Map<TransactionModel>(request);
        transactionModel.Id = Guid.NewGuid();
        transactionModel.TransactionType = TransactionType.Deposit;

        _transactionServiceMock.Setup(s => s.CreateDepositTransaction(
            It.Is<TransactionModel>(t =>
            t.AccountId == request.AccountId &&
            t.Amount == request.Amount &&
            t.TransactionType == TransactionType.Deposit)))
            .ReturnsAsync(transactionModel.Id);

        // Act
        var result = await _sut.CreateDepositTransaction(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var response = okResult.Value as Guid?;
        response.Should().Be(transactionModel.Id);
    }

    [Fact]
    public async Task CreateWithdrawalTransaction_ReturnsGuid()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            Currency = Currency.USD,
            Amount = 100
        };
        var transactionModel = mapper.Map<TransactionModel>(request);
        transactionModel.Id = Guid.NewGuid();
        transactionModel.TransactionType = TransactionType.Withdrawal;

        _transactionServiceMock.Setup(s => s.CreateWithdrawalTransaction(
            It.Is<TransactionModel>(t =>
            t.AccountId == request.AccountId &&
            t.Amount == request.Amount &&
            t.TransactionType == TransactionType.Withdrawal)))
            .ReturnsAsync(transactionModel.Id);

        // Act
        var result = await _sut.CreateWithdrawalTransaction(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var response = okResult.Value as Guid?;
        response.Should().Be(transactionModel.Id);
    }

    [Fact]
    public async Task CreateTransferTransaction_ReturnsGuids()
    {
        // Arrange
        var request = new CreateTransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 100,
            FromCurrency = Currency.USD,
            ToCurrency = Currency.EUR
        };
        var transferModel = mapper.Map<TransferTransactionModel>(request);
        var transactionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _transactionServiceMock.Setup(s => s.CreateTransferTransaction(
            It.Is<TransferTransactionModel>(t =>
            t.FromAccountId == request.FromAccountId &&
            t.ToAccountId == request.ToAccountId &&
            t.Amount == request.Amount &&
            t.FromCurrency == request.FromCurrency &&
            t.ToCurrency == request.ToCurrency)))
            .ReturnsAsync(transactionIds);

        // Act
        var result = await _sut.CreateTransferTransaction(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var response = okResult.Value as List<Guid>;
        response.Should().BeEquivalentTo(transactionIds);
    }
}
