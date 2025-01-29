using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Domain.Controllers;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Tests;
public class AccountControllerTests
{
    private readonly Mock<ILogger<AccountController>> _loggerMock;
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly IMapper _mapper;
    private readonly AccountController _sut;

    public AccountControllerTests()
    {
        _loggerMock = new Mock<ILogger<AccountController>>();
        _transactionServiceMock = new Mock<ITransactionService>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AccountBalanceModel, BalanceResponse>();
            cfg.CreateMap<TransactionModel, TransactionResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TransactionType));
        });
        _mapper = config.CreateMapper();
        _sut = new AccountController(_loggerMock.Object, _transactionServiceMock.Object, _mapper);
    }

    [Fact]
    public async Task GetBalance_ReturnsBalanceResponse()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accountBalanceModel = new AccountBalanceModel
        {
            AccountId = accountId,
            Balance = 1000
        };
        var balanceResponse = _mapper.Map<BalanceResponse>(accountBalanceModel);

        _transactionServiceMock.Setup(s => s.CountAccountBalance(accountId))
            .ReturnsAsync(accountBalanceModel);

        // Act
        var result = await _sut.GetBalance(accountId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var response = okResult.Value as BalanceResponse;
        response.Should().BeEquivalentTo(balanceResponse);
    }

    [Fact]
    public async Task GetTransactions_ReturnsTransactionResponses()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transferDate = DateTime.UtcNow;
        var transactionId = Guid.NewGuid();
        var relatedTransactionId = Guid.NewGuid();
        var transactionModels = new List<TransactionModel>
            {
                new()
                {
                    Id = transactionId,
                    AccountId = accountId,
                    Amount = 100,
                    TransactionType = TransactionType.Deposit,
                    Date = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    Amount = 200,
                    TransactionType = TransactionType.Withdrawal,
                    Date = DateTime.UtcNow
                },
                new()
                {
                    Id = transactionId,
                    AccountId = accountId,
                    Amount = 100,
                    TransactionType = TransactionType.Transfer,
                    Date = transferDate,
                    RelatedTransaction = new TransactionModel
                    {
                        Id = relatedTransactionId,
                        AccountId = Guid.NewGuid(),
                        Amount = 100,
                        Date = transferDate,
                        TransactionType = TransactionType.Transfer
                    }
                }
            };
        var transactionResponses = _mapper.Map<List<TransactionResponse>>(transactionModels);

        _transactionServiceMock.Setup(s => s.GetTransactionsByAccountIdAsync(accountId))
            .ReturnsAsync(transactionModels);

        // Act
        var result = await _sut.GetTransactions(accountId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var response = okResult.Value as List<TransactionResponse>;
        response.Should().BeEquivalentTo(transactionResponses);
    }
}
