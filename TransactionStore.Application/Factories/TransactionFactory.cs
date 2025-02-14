using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Messages;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Factories;

public class TransactionFactory(
        IMapper mapper,
        ILogger<TransactionFactory> logger,
        IConfiguration configuration,
        ICurrencyExchangeClient currencyExchangeClient)
    : ITransactionFactory
{
    public Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType, decimal commission)
    {
        var transaction = mapper.Map<Transaction>(transactionModel);
        transaction.TransactionType = transactionType;
        if (transactionType == TransactionType.Deposit)
            transaction.Amount -= commission;
        else
            transaction.Amount = -transaction.Amount;
        logger.LogDebug("Created transaction model: {Transaction}", transaction);
        return transaction;
    }

    public TransactionCreatedMessage CreateTransactionMessage(Transaction transaction, CurrencyType currency, decimal commission)
    {
        logger.LogDebug("Entering CreateTransactionMessage with transaction: {Transaction}, currency: {Currency}, commission: {Commission}", transaction, currency, commission);

        var amountRUB = ConvertToRUB(transaction.Amount, currency);
        var message = new TransactionCreatedMessage
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            AmountRUB = amountRUB,
            Commission = commission,
            Date = transaction.Date,
            TransactionType = transaction.TransactionType
        };

        return message;
    }

    public TransferTransactionResult CreateTransferTransactions(TransferTransactionModel transferModel, decimal exchangeRate, decimal commission)
    {
        logger.LogDebug("Entering CreateTransferTransactions with transferModel: {TransferModel}, exchangeRate: {ExchangeRate}, commission: {Commission}", transferModel, exchangeRate, commission);

        var amountFrom = transferModel.Amount;
        var amountTo = (amountFrom-commission) * exchangeRate;
        var transFrom = CreateTransferTransactionEntity(transferModel.FromAccountId, -amountFrom, TransactionType.Transfer);
        var transTo = CreateTransferTransactionEntity(transferModel.ToAccountId, amountTo, TransactionType.Transfer);

        return new TransferTransactionResult
        {
            TransFrom = transFrom,
            TransTo = transTo
        };
    }

    public TransferTransactionResult CreateTransferTransactionMessages(TransferTransactionResult transferResult, TransferTransactionModel transferModel, decimal commission)
    {
        logger.LogDebug("Entering CreateTransferTransactionMessages with transferResult: {TransferResult}, transferModel: {TransferModel}, commission: {Commission}", transferResult, transferModel, commission);

        var transFromMessage = CreateTransactionMessage(transferResult.TransFrom, transferModel.FromCurrency, commission);
        var transToMessage = CreateTransactionMessage(transferResult.TransTo, transferModel.ToCurrency, 0);

        transferResult.TransFromMessage = transFromMessage;
        transferResult.TransToMessage = transToMessage;

        return transferResult;
    }

    public decimal CalculateCommission(decimal amount, TransactionType transactionType)
    {
        string commissionKey = GetCommissionKey(transactionType);

        var commissionRate = configuration.GetValue<decimal?>(commissionKey);
        if (commissionRate == null || commissionRate.Value < 0)
        {
            logger.LogCritical($"{commissionKey} is not configured or negative");
            throw new CommissionSettingException($"{commissionKey} is not configured or went broken.");
        }
        return amount * (commissionRate.Value / 100);
    }

    private Transaction CreateTransferTransactionEntity(Guid accountId, decimal amount, TransactionType transactionType)
    {
        return new Transaction
        {
            AccountId = accountId,
            Amount = amount,
            TransactionType = transactionType
        };
    }

    private string GetCommissionKey(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.Deposit => "TransactionCommissionRates:Deposit",
            TransactionType.Withdrawal => "TransactionCommissionRates:Withdrawal",
            TransactionType.Transfer => "TransactionCommissionRates:Transfer",
            _ => throw new CommissionSettingException("Unsupported transaction type of transaction")
        };
    }

    private decimal ConvertToRUB(decimal amount, CurrencyType currency)
    {
        if (currency == CurrencyType.RUB)
        {
            return amount;
        }

        var exchangeRate = currencyExchangeClient.GetExchangeRate(currency, CurrencyType.RUB);
        if (exchangeRate <= 0)
        {
            logger.LogCritical("Invalid exchange rate for Currency: {Currency}", currency);
            throw new InvalidExchangeRateException("Invalid exchange rate");
        }

        return amount * exchangeRate;
    }
}
