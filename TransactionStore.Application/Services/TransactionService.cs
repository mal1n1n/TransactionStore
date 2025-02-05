using AutoMapper;
using Microsoft.Extensions.Logging;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Application.Services;
public class TransactionService(
        ITransactionRepository transactionRepository,
        IMapper mapper, 
        ICurrencyExchangeClient currencyExchangeClient,
        ITransactionFactory transactionFactory,
        ILogger<TransactionService> logger) 
    : ITransactionService
{
    public async Task<TransactionModel> GetTransactionByIdAsync(Guid id)
    {
        logger.LogDebug("Entering GetTransactionByIdAsync with id: {Id}", id);
        var transaction = await transactionRepository.GetByIdAsync(id);
        if (transaction == null)
        {
            logger.LogError($"Transaction with id {id} not found");
            throw new TransactionNotFoundException($"Transaction with id {id} not found");
        }
            
        var transactionModel = mapper.Map<TransactionModel>(transaction);
        logger.LogDebug("Mapped transaction to transactionModel: {TransactionModel}", transactionModel);

        if (transaction.TransactionType == TransactionType.Transfer)
        {
            var relatedTransaction = await transactionRepository.FindAsync(t =>
                t.Date == transaction.Date && 
                t.Id != transaction.Id && 
                t.TransactionType == TransactionType.Transfer);

            if (relatedTransaction.Any())
            {
                var related = relatedTransaction.First();
                transactionModel.RelatedTransaction = mapper.Map<TransactionModel>(related);
                logger.LogDebug("Found and mapped related transaction: {RelatedTransaction}", related);
            }
            else
            {
                logger.LogCritical("Related transaction for transaction {Id} not found", transaction.Id);
                throw new TransactionNotFoundException($"Related transaction for transaction {transaction.Id} not found");
            }
        }
        logger.LogInformation("GetTransactionByIdAsync successful for id: {Id}", id);
        return transactionModel;
    }
    public async Task<AccountBalanceModel> CountAccountBalance(Guid accountId)
    {
        logger.LogDebug("Entering CountAccountBalance with accountId: {AccountId}", accountId);
        var balance = await GetAccountBalance(accountId);
        var balanceModel = new AccountBalanceModel()
        {
            AccountId = accountId,
            Balance = balance
        };
        logger.LogInformation("CountAccountBalance successful for accountId: {AccountId}, balance: {Balance}", accountId, balance);
        return balanceModel;
    }
    public async Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId)
    {
        logger.LogDebug("Entering GetTransactionsByAccountIdAsync with accountId: {AccountId}", accountId);
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);
        logger.LogDebug("Found transactions: {Transactions} for accountId: {AccountId}", transactions, accountId);

        // Получаем все даты трансферных транзакций
        var transferDates = transactions
            .Where(t => t.TransactionType == TransactionType.Transfer)
            .Select(t => t.Date)
            .Distinct()
            .ToList();
        logger.LogTrace("Transfer dates: {TransferDates} for accountId: {AccountId}", transferDates, accountId);

        // Получаем все трансферные транзакции
        var allTransferTransactions = await transactionRepository.FindAsync(t =>
            transferDates.Contains(t.Date) &&
            t.TransactionType == TransactionType.Transfer);
        logger.LogTrace("All transfer transactions: {AllTransferTransactions} for accountId: {AccountId}", allTransferTransactions, accountId);

        // Фильтруем связанные трансферные транзакции
        var relatedTransactions = allTransferTransactions
            .Where(t => t.AccountId != accountId)
            .ToList();
        logger.LogTrace("Related transactions: {RelatedTransactions} for accountId: {AccountId}", relatedTransactions, accountId);

        if (relatedTransactions == null)
        {
            logger.LogCritical("No related transactions found for account {AccountId}", accountId);
            throw new TransactionNotFoundException($"No related transactions found for account {accountId}");
        }

        // Словарь для быстрого поиска связанных транзакций
        var relatedTransactionsDict = relatedTransactions
            .GroupBy(t => t.Date)
            .ToDictionary(g => g.Key, g => g.First());
        logger.LogTrace("Related transactions dictionary: {RelatedTransactionsDict} for accountId: {AccountId}", relatedTransactionsDict, accountId);

        // Формируем список транзакций и добавляем связанные транзакции для трансферных
        var transactionModels = transactions.Select(transaction =>
        {
            var transactionModel = mapper.Map<TransactionModel>(transaction);
            if (transaction.TransactionType == TransactionType.Transfer)
            {
                relatedTransactionsDict.TryGetValue(transaction.Date, out var relatedTransactionModel);
                if (relatedTransactionModel == null)
                {
                    logger.LogCritical("Related transaction for transaction {Id} not found", transaction.Id);
                    throw new TransactionNotFoundException($"Related transaction for transaction {transaction.Id} not found");
                }
                transactionModel.RelatedTransaction = mapper.Map<TransactionModel>(relatedTransactionModel);
                logger.LogTrace("Mapped related transaction: {RelatedTransactionModel} for transaction: {Transaction}", relatedTransactionModel, transaction);
            }
            return transactionModel;
        }).ToList();

        // Сортировка транзакций по дате от актуальной к старой
        transactionModels = [.. transactionModels.OrderByDescending(t => t.Date)];
        logger.LogInformation("GetTransactionsByAccountIdAsync successful for accountId: {AccountId}", accountId);
        return transactionModels;
    }

    public async Task<Guid> CreateTransaction(TransactionModel transactionModel,TransactionType transactionType)
    {
        logger.LogDebug("Entering CreateTransaction with AccountId: {AccountId}, Amount: {Amount}, TransactionType: {TransactionType}", transactionModel.AccountId, transactionModel.Amount, transactionType);

        if (transactionType == TransactionType.Withdrawal)
        {
            var balance = await GetAccountBalance(transactionModel.AccountId);
            if (balance < transactionModel.Amount)
            {
                logger.LogError("Insufficient funds for AccountId: {AccountId}", transactionModel.AccountId);
                throw new InvalidOperationException("Insufficient funds");
            }
        }

        var transaction = transactionFactory.CreateTransaction(transactionModel, transactionType);
        await transactionRepository.AddAsync(transaction);
        logger.LogInformation("CreateTransaction successful for AccountId: {AccountId}, Amount: {Amount}, TransactionType: {TransactionType}", transactionModel.AccountId, transactionModel.Amount, transactionType);
        return transaction.Id;
    }

    public async Task<IEnumerable<Guid>> CreateTransferTransaction(TransferTransactionModel transferModel)
    {
        logger.LogDebug("Entering CreateTransferTransaction with FromAccountId: {FromAccountId}, ToAccountId: {ToAccountId}, Amount: {Amount}", transferModel.FromAccountId, transferModel.ToAccountId, transferModel.Amount);
        
        var fromAccountBalance = await GetAccountBalance(transferModel.FromAccountId);
        if (fromAccountBalance < transferModel.Amount)
        {
            logger.LogError("Insufficient funds for FromAccountId: {FromAccountId}", transferModel.FromAccountId);
            throw new InvalidOperationException("Insufficient funds");
        }

        var exchangeRate = await currencyExchangeClient.GetExchangeRateAsync(transferModel.FromCurrency, transferModel.ToCurrency);
        if (exchangeRate <= 0)
        {
            logger.LogCritical("Invalid exchange rate for FromCurrency: {FromCurrency}, ToCurrency: {ToCurrency}", transferModel.FromCurrency, transferModel.ToCurrency);
            throw new InvalidOperationException("Invalid exchange rate");
        }
        var (transFrom, transTo) = transactionFactory.CreateTransferTransaction(transferModel, exchangeRate);
        await transactionRepository.TransactionalAddRangeAsync([transFrom, transTo]);
        logger.LogInformation("CreateTransferTransaction successful for FromAccountId: {FromAccountId}, ToAccountId: {ToAccountId}, Amount: {Amount}", transferModel.FromAccountId, transferModel.ToAccountId, transferModel.Amount);
        return [transFrom.Id, transTo.Id];
    }
    private async Task<decimal> GetAccountBalance(Guid accountId)
    {
        logger.LogDebug("Entering GetAccountBalance with accountId: {AccountId}", accountId);
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);
        if (!transactions.Any())
        {
            logger.LogWarning("No transactions found for account {AccountId} when calculating balance", accountId);
        }
        var balance = transactions.Sum(t => t.Amount);
        logger.LogDebug("Calculated balance: {Balance} for accountId: {AccountId}", balance, accountId);
        logger.LogInformation("GetAccountBalance successful for accountId: {AccountId}, balance: {Balance}", accountId, balance);
        return balance;
    }
}