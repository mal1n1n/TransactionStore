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

        return mapper.Map<TransactionModel>(transaction);
    }
    public async Task<AccountBalanceModel> CountAccountBalance(Guid accountId)
    {
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);
        var balanceModel = new AccountBalanceModel()
        {
            AccountId = accountId,
            Balance = transactions.Sum(t => t.Amount)
        };
        return balanceModel;
    }
    public async Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId)
    {
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);
        // Получение всех связанных трансферных транзакций
        var transferTransactions = transactions
            .Where(t => t.TransactionType == TransactionType.Transfer)
            .Select(t => t.Date)
            .Distinct();
        
        var relatedTransactions = await transactionRepository.FindAsync(t =>
            transferTransactions.Contains(t.Date) &&
            t.AccountId != accountId);

        // Объединение найденных транзакций
        var allTransactions = transactions
            .Concat(relatedTransactions)
            .ToList();
        // Хз,моя ли задача сортировать найденные транзакции аккаунта, но вот
        // Сортировка транзакций по дате от актуальной к старой (если трансфер - сначала идут входящие, потом исходящие)
        allTransactions = allTransactions
            .OrderByDescending(t => t.Date)
            .ThenBy(t => t.TransactionType == TransactionType.Transfer && t.Amount > 0 ? 0 : 1)
            .ToList();

        return mapper.Map<IEnumerable<TransactionModel>>(allTransactions);
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
        await transactionRepository.AddRangeAsync([transFrom, transTo]);
        return [transFrom.Id, transTo.Id];
    }
}