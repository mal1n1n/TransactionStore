using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
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
        var transactionModel = new TransactionModel { 
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TransactionResponse>(okResult.Value);
        Assert.Equal(transactionId, response.Id);
        Assert.Equal(transactionModel.AccountId, response.AccountId);
        Assert.Equal(transactionModel.Amount, response.Amount);
        Assert.Equal(transactionModel.TransactionType, response.Type);
        Assert.Equal(transactionModel.Date, response.Date);
    }

    [Fact]
    public async Task CreateDepositTransaction_ReturnsGuid()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100
        };
        var transactionModel = mapper.Map<TransactionModel>(request);
        transactionModel.Id = Guid.NewGuid();
        transactionModel.TransactionType = TransactionType.Deposit;

        _transactionServiceMock.Setup(s => s.CreateTransaction(
            It.Is<TransactionModel>(t =>
            t.AccountId == request.AccountId &&
            t.Amount == request.Amount &&
            t.TransactionType == TransactionType.Deposit),
            TransactionType.Deposit))
            .ReturnsAsync(transactionModel.Id);

        // Act
        var result = await _sut.CreateDepositTransaction(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<Guid>(okResult.Value);
        Assert.Equal(transactionModel.Id, response);
    }

    [Fact]
    public async Task CreateWithdrawalTransaction_ReturnsGuid()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            Amount = 100
        };
        var transactionModel = mapper.Map<TransactionModel>(request);
        transactionModel.Id = Guid.NewGuid();
        transactionModel.TransactionType = TransactionType.Withdrawal;

        _transactionServiceMock.Setup(s => s.CreateTransaction(
            It.Is<TransactionModel>(t =>
            t.AccountId == request.AccountId &&
            t.Amount == request.Amount &&
            t.TransactionType == TransactionType.Withdrawal),
            TransactionType.Withdrawal))
            .ReturnsAsync(transactionModel.Id);

        // Act
        var result = await _sut.CreateWithdrawalTransaction(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<Guid>(okResult.Value);
        Assert.Equal(transactionModel.Id, response);
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
            FromCurrency = CurrencyType.USD,
            ToCurrency = CurrencyType.EUR
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
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<List<Guid>>(okResult.Value);
        Assert.Equal(transactionIds, response);
    }
}
