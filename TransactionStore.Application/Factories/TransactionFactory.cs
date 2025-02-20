using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;
using MYPBackendMicroserviceIntegrations.Messages;
using MYPBackendMicroserviceIntegrations.Enums;

namespace TransactionStore.Application.Factories;

public class TransactionFactory(
        IMapper mapper,
        ILogger<TransactionFactory> logger,
        ICurrencyExchangeClient currencyExchangeClient,
        ICommissionUtils commissionUtils)
    : ITransactionFactory
{
    public Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType)
    {
        var transaction = mapper.Map<Transaction>(transactionModel);
        var commission = commissionUtils.CalculateCommission(transaction.Amount, transactionType);
        transaction.TransactionType = transactionType;
        if (transactionType == TransactionType.Deposit)
            transaction.Amount -= commission;
        else
            transaction.Amount = -transaction.Amount;
        logger.LogDebug("Created transaction model: {Transaction}", transaction);
        return transaction;
    }
    public TransactionCreatedMessage CreateTransactionMessage(Transaction transaction, CurrencyType currency)
    {
        logger.LogDebug("Entering CreateTransactionMessage with transaction: {Transaction}, currency: {Currency}", transaction, currency);
        var amountRub = currencyExchangeClient.GetExchangeRate(currency, CurrencyType.RUB) * transaction.Amount;
        var commission = commissionUtils.CalculateCommission(transaction.Amount, transaction.TransactionType);
        var message = new TransactionCreatedMessage
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            AmountRub = amountRub,
            Commission = commission,
            Date = transaction.Date,
            TransactionType = transaction.TransactionType
        };

        return message;
    }

    public TransferTransactionResult CreateTransferTransactions(TransferTransactionModel transferModel)
    {
        logger.LogDebug("Entering CreateTransferTransactions with transferModel: {TransferModel}", transferModel);

        var exchangeRate = currencyExchangeClient.GetExchangeRate(transferModel.FromCurrency, transferModel.ToCurrency);
        var commission = commissionUtils.CalculateCommission(transferModel.Amount, TransactionType.Transfer);
        var amountFrom = transferModel.Amount;
        var amountTo = (amountFrom-commission) * exchangeRate;
        var transFrom = new Transaction(transferModel.FromAccountId, -amountFrom, TransactionType.Transfer);
        var transTo = new Transaction(transferModel.ToAccountId, amountTo, TransactionType.Transfer);

        return new TransferTransactionResult
        {
            TransFrom = transFrom,
            TransTo = transTo
        };
    }

    public TransactionCreatedMessage CreateTransferTransactionMessage(Transaction TransFrom, Transaction TransTo, TransferTransactionModel transferModel)
    {
        logger.LogDebug("Entering CreateTransferTransactionMessage for transaction: {transactionFromId}, to: {transactionToId}", TransFrom.Id, TransTo.Id);
        var transFromMessage = CreateTransactionMessage(TransFrom, transferModel.FromCurrency);
        var transToMessage = CreateTransactionMessage(TransTo, transferModel.ToCurrency);

        transToMessage.Commission = 0; // Commission is already included
        transFromMessage.RelatedTransaction = transToMessage;

        return transFromMessage;
    }
}
