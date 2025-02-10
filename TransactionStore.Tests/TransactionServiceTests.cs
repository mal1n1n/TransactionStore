using AutoMapper;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;
using System.Linq.Expressions;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Application.Services;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;
using Xunit;

namespace TransactionStore.Tests
{
    public class TransactionServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<ITransactionRepository> _transactionRepository;
        private readonly Mock<ICurrencyExchangeClient> _currencyExchangeClient;
        private readonly Mock<ITransactionFactory> _transactionFactory;
        private readonly Mock<ILogger<TransactionService>> _logger;
        private readonly TransactionService _sut;

        public TransactionServiceTests()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Transaction, TransactionModel>().ReverseMap();
            });
            _mapper = new Mapper(configuration);
            _transactionRepository = new Mock<ITransactionRepository>();
            _currencyExchangeClient = new Mock<ICurrencyExchangeClient>();
            _transactionFactory = new Mock<ITransactionFactory>();
            _logger = new Mock<ILogger<TransactionService>>();
            _sut = new TransactionService(
                _transactionRepository.Object,
                _mapper,
                _currencyExchangeClient.Object,
                _transactionFactory.Object,
                _logger.Object);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_TransactionDoesNotExist_ThrowsTransactionNotFoundException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _sut.GetTransactionByIdAsync(transactionId);

            // Assert
            await act.Should().ThrowAsync<TransactionNotFoundException>()
                .WithMessage($"Transaction with id {transactionId} not found");
        }

        [Fact]
        public async Task GetTransactionByIdAsync_RelatedTransactionDoesNotExist_ThrowsTransactionNotFoundException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transaction = new Transaction
            {
                Id = transactionId,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Transfer
            };

            _transactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);
            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { Date = transaction.Date, Id = Guid.NewGuid(), TransactionType = TransactionType.Transfer }))))
                .ReturnsAsync([]);

            // Act
            Func<Task> act = async () => await _sut.GetTransactionByIdAsync(transactionId);

            // Assert
            await act.Should().ThrowAsync<TransactionNotFoundException>()
                .WithMessage($"Related transaction for transaction {transaction.Id} not found");
        }

        [Fact]
        public async Task GetTransactionByIdAsync_TransactionExists_ReturnsTransactionModel()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transaction = new Transaction
            {
                Id = transactionId,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Transfer
            };
            var transactionModel = _mapper.Map<TransactionModel>(transaction);
            var relatedTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Date = transaction.Date,
                TransactionType = TransactionType.Transfer
            };
            var transactionWithRelated = new TransactionModel
            {
                Id = transactionId,
                Date = transaction.Date,
                TransactionType = TransactionType.Transfer,
                RelatedTransaction = _mapper.Map<TransactionModel>(relatedTransaction)
            };

            _transactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);
            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { Date = transaction.Date, Id = Guid.NewGuid(), TransactionType = TransactionType.Transfer }))))
                .ReturnsAsync([relatedTransaction]);

            // Act
            var result = await _sut.GetTransactionByIdAsync(transactionId);

            // Assert
            result.Should().BeEquivalentTo(transactionWithRelated);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_DepositTransactionExists_ReturnsTransactionModel()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transaction = new Transaction
            {
                Id = transactionId,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Deposit
            };
            var transactionModel = _mapper.Map<TransactionModel>(transaction);

            _transactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            // Act
            var result = await _sut.GetTransactionByIdAsync(transactionId);

            // Assert
            result.Should().BeEquivalentTo(transactionModel);
        }

        [Fact]
        public async Task CountAccountBalance_NoTransactions_ReturnsZeroBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = accountId }))))
                .ReturnsAsync([]);

            // Act
            var result = await _sut.CountAccountBalance(accountId);

            // Assert
            result.AccountId.Should().Be(accountId);
            result.Balance.Should().Be(0);
        }

        [Fact]
        public async Task CountAccountBalance_TransactionsExist_ReturnsCorrectBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var transactions = new List<Transaction>
            {
                new() { AccountId = accountId, Amount = 100 },
                new() { AccountId = accountId, Amount = 200 },
                new() { AccountId = accountId, Amount = -50 }
            };

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = accountId }))))
                .ReturnsAsync(transactions);

            // Act
            var result = await _sut.CountAccountBalance(accountId);

            // Assert
            result.AccountId.Should().Be(accountId);
            result.Balance.Should().Be(250);
        }

        [Fact]
        public async Task GetTransactionsByAccountIdAsync_NoTransactions_ReturnsEmptyList()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = accountId }))))
                .ReturnsAsync([]);

            // Act
            var result = await _sut.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTransactionsByAccountIdAsync_TransactionsExist_ReturnsTransactionModels()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var transactions = new List<Transaction>
            {
                new() { Id = Guid.NewGuid(), AccountId = accountId, Amount = 100, Date = DateTime.UtcNow, TransactionType = TransactionType.Deposit },
                new() { Id = Guid.NewGuid(), AccountId = accountId, Amount = 200, Date = DateTime.UtcNow, TransactionType = TransactionType.Withdrawal }
            };
            var transactionModels = _mapper.Map<List<TransactionModel>>(transactions);

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = accountId }))))
                .ReturnsAsync(transactions);

            // Act
            var result = await _sut.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            result.Should().BeEquivalentTo(transactionModels);
        }

        [Fact]
        public async Task GetTransactionsByAccountIdAsync_TransferTransactionsExist_ReturnsTransactionModelsWithRelatedTransactions()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var transferDate = DateTime.UtcNow;
            var transactionId = Guid.NewGuid();
            var relatedTransactionId = Guid.NewGuid();
            var transactions = new List<Transaction>
            {
                new()
                {
                    Id = transactionId,
                    AccountId = accountId,
                    Amount = 100,
                    Date = transferDate,
                    TransactionType = TransactionType.Transfer
                }
            };
            var relatedTransactions = new List<Transaction>
            {
                new()
                {
                    Id = relatedTransactionId,
                    AccountId = Guid.NewGuid(),
                    Amount = 100,
                    Date = transferDate,
                    TransactionType = TransactionType.Transfer
                }
            };
            var transactionModels = _mapper.Map<List<TransactionModel>>(transactions);
            transactionModels.First().RelatedTransaction = _mapper.Map<TransactionModel>(relatedTransactions.First());

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = accountId }))))
                .ReturnsAsync(transactions);
            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { Date = transferDate, Id = relatedTransactionId, TransactionType = TransactionType.Transfer }))))
                .ReturnsAsync(relatedTransactions);

            // Act
            var result = await _sut.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            result.Should().BeEquivalentTo(transactionModels);
        }

        [Fact]
        public async Task GetTransactionsByAccountIdAsync_NoRelatedTransactions_ThrowsTransactionNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var transferDate = DateTime.UtcNow;
            var transactions = new List<Transaction>
            {
                new() { Id = Guid.NewGuid(), AccountId = accountId, Amount = 100, Date = transferDate, TransactionType = TransactionType.Transfer }
            };

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = accountId }))))
                .ReturnsAsync(transactions);
            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { Date = transferDate, Id = Guid.NewGuid(), TransactionType = TransactionType.Transfer }))))
                .ReturnsAsync([]);

            // Act
            Func<Task> act = async () => await _sut.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            await act.Should().ThrowAsync<TransactionNotFoundException>()
                .WithMessage($"Related transaction for transaction {transactions.First().Id} not found");
        }

        [Fact]
        public async Task CreateTransaction_InsufficientFunds_ThrowsInvalidOperationException()
        {
            // Arrange
            var transactionModel = new TransactionModel
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Withdrawal
            };

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = transactionModel.AccountId }))))
                .ReturnsAsync(new List<Transaction> { new() { AccountId = transactionModel.AccountId, Amount = 50 } });

            // Act
            Func<Task> act = async () => await _sut.CreateTransaction(transactionModel, TransactionType.Withdrawal);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Insufficient funds");

            _transactionRepository.Verify(repo => repo.FindAsync(It.IsAny<Expression<Func<Transaction, bool>>>()), Times.Once);
            _transactionRepository.Verify(repo => repo.ExecuteStoredProcedureWithTransactionAsync(It.IsAny<string>(), It.IsAny<DynamicParameters>()), Times.Never);
        }

        [Fact]
        public async Task CreateTransaction_ValidDepositTransaction_ReturnsTransactionId()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transactionModel = new TransactionModel
            {
                Id = transactionId,
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Deposit
            };
            var transaction = _mapper.Map<Transaction>(transactionModel);

            _transactionFactory.Setup(factory => factory.CreateTransaction(transactionModel, TransactionType.Deposit))
                .Returns(transaction);
            _transactionRepository.Setup(repo => repo.AddAsync(transaction))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateTransaction(transactionModel, TransactionType.Deposit);

            // Assert
            result.Should().Be(transactionId);
            _transactionFactory.Verify(factory => factory.CreateTransaction(transactionModel, TransactionType.Deposit), Times.Once);
            _transactionRepository.Verify(repo => repo.AddAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task CreateTransaction_ValidWithdrawalTransaction_ReturnsTransactionId()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transactionModel = new TransactionModel
            {
                Id = transactionId,
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Withdrawal
            };
            var transaction = _mapper.Map<Transaction>(transactionModel);

            _transactionFactory.Setup(factory => factory.CreateTransaction(transactionModel, TransactionType.Withdrawal))
                .Returns(transaction);
            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = transactionModel.AccountId }))))
                .ReturnsAsync(new List<Transaction> { new() { AccountId = transactionModel.AccountId, Amount = 200 } });
            _transactionRepository.Setup(repo => repo.ExecuteStoredProcedureWithTransactionAsync(
                It.Is<string>(s => s == "public.sp_createtransaction"),
                It.IsAny<DynamicParameters>()))
                .Callback<string, DynamicParameters>((storedProcedure, parameters) =>
                {
                    parameters.Add("outputid", transactionId, DbType.Guid, ParameterDirection.Output);
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateTransaction(transactionModel, TransactionType.Withdrawal);

            // Assert
            result.Should().Be(transactionId);
            _transactionFactory.Verify(factory => factory.CreateTransaction(transactionModel, TransactionType.Withdrawal), Times.Once);
            _transactionRepository.Verify(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = transactionModel.AccountId }))), Times.Once);
            _transactionRepository.Verify(repo => repo.ExecuteStoredProcedureWithTransactionAsync(
                It.Is<string>(s => s == "public.sp_createtransaction"),
                It.IsAny<DynamicParameters>()), Times.Once);
        }
        [Fact]
        public async Task CreateTransferTransaction_InsufficientFunds_ThrowsInvalidOperationException()
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

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = transferModel.FromAccountId }))))
                .ReturnsAsync([new() { AccountId = transferModel.FromAccountId, Amount = 50 }]);

            // Act
            Func<Task> act = async () => await _sut.CreateTransferTransaction(transferModel);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Insufficient funds");
        }

        [Fact]
        public async Task CreateTransferTransaction_ValidTransfer_ReturnsTransactionIds()
        {
            // Arrange
            var fromTransactionId = Guid.NewGuid();
            var toTransactionId = Guid.NewGuid();
            var transferModel = new TransferTransactionModel
            {
                FromAccountId = Guid.NewGuid(),
                ToAccountId = Guid.NewGuid(),
                Amount = 100,
                FromCurrency = CurrencyType.USD,
                ToCurrency = CurrencyType.EUR
            };
            var exchangeRate = 0.85m;
            var fromTransaction = new Transaction
            {
                Id = fromTransactionId,
                AccountId = transferModel.FromAccountId,
                Amount = transferModel.Amount,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Transfer
            };
            var toTransaction = new Transaction
            {
                Id = toTransactionId,
                AccountId = transferModel.ToAccountId,
                Amount = transferModel.Amount * exchangeRate,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.Transfer
            };

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = transferModel.FromAccountId }))))
                .ReturnsAsync([new() { AccountId = transferModel.FromAccountId, Amount = 200 }]);

            _currencyExchangeClient.Setup(client => client.GetExchangeRateAsync(transferModel.FromCurrency, transferModel.ToCurrency))
                .ReturnsAsync(exchangeRate);
            _transactionFactory.Setup(factory => factory.CreateTransferTransaction(transferModel, exchangeRate))
                .Returns((fromTransaction, toTransaction));
            _transactionRepository.Setup(repo => repo.ExecuteStoredProcedureWithTransactionAsync(
                It.Is<string>(s => s == "public.sp_createtransfertransaction"),
                It.IsAny<DynamicParameters>()))
                .Callback<string, DynamicParameters>((storedProcedure, parameters) =>
                {
                    parameters.Add("outputfromid", fromTransactionId, DbType.Guid, ParameterDirection.Output);
                    parameters.Add("outputtoid", toTransactionId, DbType.Guid, ParameterDirection.Output);
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateTransferTransaction(transferModel);

            // Assert
            result.Should().Contain(fromTransactionId);
            result.Should().Contain(toTransactionId);
            _transactionFactory.Verify(factory => factory.CreateTransferTransaction(transferModel, exchangeRate), Times.Once);
            _transactionRepository.Verify(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = transferModel.FromAccountId }))), Times.Once);
            _transactionRepository.Verify(repo => repo.ExecuteStoredProcedureWithTransactionAsync(
                It.Is<string>(s => s == "public.sp_createtransfertransaction"),
                It.IsAny<DynamicParameters>()), Times.Once);
        }

        [Fact]
        public async Task CreateTransferTransaction_InvalidExchangeRate_ThrowsInvalidOperationException()
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
            var exchangeRate = 0m;

            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(new Transaction { AccountId = transferModel.FromAccountId }))))
                .ReturnsAsync([new() { AccountId = transferModel.FromAccountId, Amount = 200 }]);

            _currencyExchangeClient.Setup(client => client.GetExchangeRateAsync(transferModel.FromCurrency, transferModel.ToCurrency))
                .ReturnsAsync(exchangeRate);

            // Act
            Func<Task> act = async () => await _sut.CreateTransferTransaction(transferModel);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid exchange rate");
        }
    }
}


