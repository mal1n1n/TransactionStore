using AutoMapper;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Factories;
public class TransactionFactory(
        IMapper mapper) 
    : ITransactionFactory
{
    public Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType)
    {
        var transaction = mapper.Map<Transaction>(transactionModel);
        transaction.TransactionType = transactionType;
        if (transactionType == TransactionType.Withdrawal)
        {
            transaction.Amount = -transaction.Amount;
        }
        return transaction;
    }

    public (Transaction transFrom, Transaction transTo) CreateTransferTransaction(TransferTransactionModel transferModel, decimal exchangeRate)
    {
        var transFrom = new Transaction
        {
            AccountId = transferModel.FromAccountId,
            Amount = -transferModel.Amount,
            TransactionType = TransactionType.Transfer
        };
        var transTo = new Transaction
        {
            AccountId = transferModel.ToAccountId,
            Amount = transferModel.Amount * exchangeRate,
            TransactionType = TransactionType.Transfer
        };
        return (transFrom, transTo);
    }
}
