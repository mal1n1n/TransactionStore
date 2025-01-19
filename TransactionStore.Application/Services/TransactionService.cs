using AutoMapper;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Application.Services;
public class TransactionService(
        ITransactionRepository transactionRepository,
        IMapper mapper, 
        ICurrencyExchangeClient currencyExchangeClient) 
    : ITransactionService
{
    public async Task<TransactionModel?> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await transactionRepository.GetByIdAsync(id);
        return mapper.Map<TransactionModel>(transaction);
    }
    public async Task<decimal> CountUserBalance(Guid accountId)
    {
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);
        return transactions.Sum(t => t.Amount);
    }
    public async Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId)
    {
        var transactions = await transactionRepository.FindAsync(t => t.AccountId == accountId);
        return mapper.Map<IEnumerable<TransactionModel>>(transactions);
    }

    public async Task<Guid> CreateTransaction(TransactionModel transactionModel,TransactionType transactionType)
    {
        var transaction = mapper.Map<Transaction>(transactionModel);
        transaction.TransactionType = transactionType; //мб тут сделать factory для создания транзакций?
        await transactionRepository.AddAsync(transaction);
        return transaction.Id;
    }

    public async Task<IEnumerable<Guid>> CreateTransferTransaction(TransferTransactionModel transferModel)
    {
        var course = await currencyExchangeClient.GetExchangeRateAsync(transferModel.FromCurrency, transferModel.ToCurrency);
        var transFrom = new Transaction
        {
            AccountId = transferModel.FromAccountId,
            Amount = -transferModel.Amount,
            TransactionType = TransactionType.Transfer
        };
        var transTo = new Transaction
        {
            AccountId = transferModel.ToAccountId,
            Amount = transferModel.Amount*course,
            TransactionType = TransactionType.Transfer
        };
        await transactionRepository.AddRangeAsync([transFrom, transTo]);
        return [transFrom.Id, transTo.Id];
    }
}