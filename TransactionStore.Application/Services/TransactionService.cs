using AutoMapper;
using System.Collections.Generic;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Application.Services;
public class TransactionService(
        ITransactionRepository transactionRepository,
        IMapper mapper, 
        ICurrencyExchangeClient currencyExchangeClient,
        ITransactionFactory transactionFactory) 
    : ITransactionService
{
    public async Task<TransactionModel> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await transactionRepository.GetByIdAsync(id);
        if (transaction == null)
            throw new TransactionNotFoundException($"Transaction with id {id} not found");

        var transactionModel = mapper.Map<TransactionModel>(transaction);

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
            }
        }
        return transactionModel;
    }
    public async Task<AccountBalanceModel> CountAccountBalance(Guid accountId)
    {
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);
        var balance = transactions.Sum(t => t.Amount);
        var balanceModel = new AccountBalanceModel()
        {
            AccountId = accountId,
            Balance = balance
        };
        return balanceModel;
    }
    public async Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId)
    {
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);

        // Получаем все даты трансферных транзакций
        var transferDates = transactions
            .Where(t => t.TransactionType == TransactionType.Transfer)
            .Select(t => t.Date)
            .Distinct()
            .ToList();

        // Получаем все связанные трансферные транзакции
        var relatedTransactions = await transactionRepository.FindAsync(t =>
            transferDates.Contains(t.Date) && 
            t.AccountId != accountId && 
            t.TransactionType == TransactionType.Transfer);

        // Словарь для быстрого поиска связанных транзакций
        var relatedTransactionsDict = relatedTransactions
            .GroupBy(t => t.Date)
            .ToDictionary(g => g.Key, g => g.First());

        // Формируем список транзакций и добавляем связанные транзакции для трансферных
        var transactionModels = transactions.Select(transaction =>
        {
            var transactionModel = mapper.Map<TransactionModel>(transaction);
            if (transaction.TransactionType == TransactionType.Transfer)
            {
                relatedTransactionsDict.TryGetValue(transaction.Date, out var relatedTransactionModel);
                if (relatedTransactionModel == null)
                    throw new TransactionNotFoundException($"Related transaction for transaction {transaction.Id} not found (Антон был прав(возможно))");
                transactionModel.RelatedTransaction = mapper.Map<TransactionModel>(relatedTransactionModel);
            }
            return transactionModel;
        }).ToList();

        // Сортировка транзакций по дате от актуальной к старой
        transactionModels = transactionModels
            .OrderByDescending(t => t.Date)
            .ToList();

        return transactionModels;
    }

    public async Task<Guid> CreateTransaction(TransactionModel transactionModel,TransactionType transactionType)
    {
        var transaction = transactionFactory.CreateTransaction(transactionModel, transactionType);
        await transactionRepository.AddAsync(transaction);
        return transaction.Id;
    }

    public async Task<IEnumerable<Guid>> CreateTransferTransaction(TransferTransactionModel transferModel)
    {
        var exchangeRate = await currencyExchangeClient.GetExchangeRateAsync(transferModel.FromCurrency, transferModel.ToCurrency);
        var (transFrom, transTo) = transactionFactory.CreateTransferTransaction(transferModel, exchangeRate);
        await transactionRepository.TransactionalAddRangeAsync([transFrom, transTo]);
        return [transFrom.Id, transTo.Id];
    }
}