using AutoMapper;
using Moq;
using System.Linq.Expressions;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Application.Services;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Tests
{
    public class TransactionServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<ITransactionRepository> _transactionRepository;
        private readonly Mock<ICurrencyExchangeClient> _currencyExchangeClient;
        private readonly Mock<ITransactionFactory> _transactionFactory;
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
            _sut = new TransactionService(
                _transactionRepository.Object,
                _mapper,
                _currencyExchangeClient.Object,
                _transactionFactory.Object);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_TransactionDoesNotExist_ThrowsTransactionNotFoundException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransactionNotFoundException>(() => _sut.GetTransactionByIdAsync(transactionId));
            Assert.Equal($"Transaction with id {transactionId} not found", exception.Message);
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
            var transactionModel = new TransactionModel { Id = transactionId };

            _transactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);
            _transactionRepository.Setup(repo => repo.FindAsync(t =>
                t.Date == transaction.Date &&
                t.Id != transaction.Id &&
                t.TransactionType == TransactionType.Transfer))
                .ReturnsAsync([]);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransactionNotFoundException>(() => _sut.GetTransactionByIdAsync(transactionId));
            Assert.Equal($"Related transaction for transaction {transaction.Id} not found", exception.Message);
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
            var transactionModel = new TransactionModel { Id = transactionId };

            _transactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);
            _transactionRepository.Setup(repo => repo.FindAsync(t =>
                t.Date == transaction.Date &&
                t.Id != transaction.Id &&
                t.TransactionType == TransactionType.Transfer))
                .ReturnsAsync([transaction]);

            // Act
            var result = await _sut.GetTransactionByIdAsync(transactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.Id);
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
            var transactionModel = new TransactionModel { Id = transactionId };

            _transactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            // Act
            var result = await _sut.GetTransactionByIdAsync(transactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.Id);
            Assert.Equal(TransactionType.Deposit, result.TransactionType);
        }

        [Fact]
        public async Task CountAccountBalance_NoTransactions_ReturnsZeroBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            _transactionRepository.Setup(repo => repo.FindAsync(t => t.AccountId == accountId))
                .ReturnsAsync([]);

            // Act
            var result = await _sut.CountAccountBalance(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.AccountId);
            Assert.Equal(0, result.Balance);
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

            _transactionRepository.Setup(repo => repo.FindAsync(t => t.AccountId == accountId))
                .ReturnsAsync(transactions);

            // Act
            var result = await _sut.CountAccountBalance(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.AccountId);
            Assert.Equal(250, result.Balance);
        }

        [Fact]
        public async Task GetTransactionsByAccountIdAsync_NoTransactions_ReturnsEmptyList()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            _transactionRepository.Setup(repo => repo.FindAsync(t => t.AccountId == accountId))
                .ReturnsAsync([]);

            // Act
            var result = await _sut.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
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

            _transactionRepository.Setup(repo => repo.FindAsync(t => t.AccountId == accountId))
                .ReturnsAsync(transactions);

            // Act
            var result = await _sut.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
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
                new() { 
                    Id = transactionId, 
                    AccountId = accountId, 
                    Amount = 100, 
                    Date = transferDate, 
                    TransactionType = TransactionType.Transfer 
                }
            };
                    var relatedTransactions = new List<Transaction>
            {
                new() { 
                    Id = relatedTransactionId, 
                    AccountId = Guid.NewGuid(), 
                    Amount = 100, 
                    Date = transferDate, 
                    TransactionType = TransactionType.Transfer 
                }
            };

            _transactionRepository.Setup(repo => repo.FindAsync(t => t.AccountId == accountId))
                .ReturnsAsync(transactions);
            
            _transactionRepository.Setup(repo => repo.FindAsync(It.Is<Expression<Func<Transaction, bool>>>(predicate =>
                predicate.Compile().Invoke(relatedTransactions.First()))))
                .ReturnsAsync(relatedTransactions);

            // Act
            var result = await _sut.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result.First().RelatedTransaction);
            Assert.Equal(relatedTransactionId, result.First().RelatedTransaction.Id);
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

            _transactionRepository.Setup(repo => repo.FindAsync(t => t.AccountId == accountId))
                .ReturnsAsync(transactions);
            _transactionRepository.Setup(repo => repo.FindAsync(t =>
                t.Date == transferDate &&
                t.AccountId != accountId &&
                t.TransactionType == TransactionType.Transfer))
                .ReturnsAsync([]);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransactionNotFoundException>(() => _sut.GetTransactionsByAccountIdAsync(accountId));
            Assert.Equal($"Related transaction for transaction {transactions.First().Id} not found", exception.Message);
        }

        [Fact]
        public async Task CreateTransaction_ValidTransaction_ReturnsTransactionId()
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
            var transaction = new Transaction
            {
                Id = transactionId,
                AccountId = transactionModel.AccountId,
                Amount = transactionModel.Amount,
                Date = transactionModel.Date,
                TransactionType = transactionModel.TransactionType
            };

            _transactionFactory.Setup(factory => factory.CreateTransaction(transactionModel, TransactionType.Deposit))
                .Returns(transaction);
            _transactionRepository.Setup(repo => repo.AddAsync(transaction))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateTransaction(transactionModel, TransactionType.Deposit);

            // Assert
            Assert.Equal(transactionId, result);
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

            _currencyExchangeClient.Setup(client => client.GetExchangeRateAsync(transferModel.FromCurrency, transferModel.ToCurrency))
                .ReturnsAsync(exchangeRate);
            _transactionFactory.Setup(factory => factory.CreateTransferTransaction(transferModel, exchangeRate))
                .Returns((fromTransaction, toTransaction));
            _transactionRepository.Setup(repo => repo.TransactionalAddRangeAsync(new[] { fromTransaction, toTransaction }))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateTransferTransaction(transferModel);

            // Assert
            Assert.Contains(fromTransactionId, result);
            Assert.Contains(toTransactionId, result);
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

            _currencyExchangeClient.Setup(client => client.GetExchangeRateAsync(transferModel.FromCurrency, transferModel.ToCurrency))
                .ReturnsAsync(exchangeRate);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTransferTransaction(transferModel));
            Assert.Equal("Invalid exchange rate", exception.Message);
        }
    }
}
