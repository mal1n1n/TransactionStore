using AutoMapper;
using Microsoft.Extensions.Logging;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Factories;
public class TransactionFactory(
        IMapper mapper,
        ILogger<TransactionFactory> logger) 
    : ITransactionFactory
{
    public Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType)
    {
        logger.LogDebug("Entering CreateTransaction with transactionModel: {TransactionModel}, transactionType: {TransactionType}", transactionModel, transactionType);
        var transaction = mapper.Map<Transaction>(transactionModel);
        transaction.TransactionType = transactionType;
        if (transactionType == TransactionType.Withdrawal)
        {
            transaction.Amount = -transaction.Amount;
        }
        logger.LogDebug("Created transaction model: {Transaction}", transaction);
        return transaction;
    }

    public (Transaction transFrom, Transaction transTo) CreateTransferTransaction(TransferTransactionModel transferModel, decimal exchangeRate)
    {
        logger.LogDebug("Entering CreateTransferTransaction with transferModel: {TransferModel}, exchangeRate: {ExchangeRate}", transferModel, exchangeRate);
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
        logger.LogDebug("Created transfer transaction models: transFrom: {TransFrom}, transTo: {TransTo}", transFrom, transTo);
        return (transFrom, transTo);
    }
}
