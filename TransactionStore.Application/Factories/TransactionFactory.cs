using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Factories;
public class TransactionFactory(
        IMapper mapper,
        ILogger<TransactionFactory> logger,
        IConfiguration configuration) 
    : ITransactionFactory
{
    public Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType, out decimal commission)
    {
        logger.LogDebug("Entering CreateTransaction with transactionModel: {TransactionModel}, transactionType: {TransactionType}", transactionModel, transactionType);
        commission = CalculateCommission(transactionModel.Amount, transactionType);
        var transaction = mapper.Map<Transaction>(transactionModel);
        transaction.TransactionType = transactionType;
        if (transactionType == TransactionType.Withdrawal)
        {
            transaction.Amount = -(transaction.Amount - commission);
        }
        else
        {
            transaction.Amount -= commission;
        }
        logger.LogDebug("Created transaction model: {Transaction}", transaction);
        return transaction;
    }

    public (Transaction transFrom, Transaction transTo, decimal commission) CreateTransferTransaction(TransferTransactionModel transferModel, decimal exchangeRate)
    {
        logger.LogDebug("Entering CreateTransferTransaction with transferModel: {TransferModel}, exchangeRate: {ExchangeRate}", transferModel, exchangeRate);

        var commission = CalculateCommission(transferModel.Amount,TransactionType.Transfer);

        var transFrom = new Transaction
        {
            AccountId = transferModel.FromAccountId,
            Amount = -(transferModel.Amount - commission),
            TransactionType = TransactionType.Transfer
        };
        var transTo = new Transaction
        {
            AccountId = transferModel.ToAccountId,
            Amount = (transferModel.Amount - commission) * exchangeRate,
            TransactionType = TransactionType.Transfer
        };

        logger.LogDebug("Created transfer transaction models: transFrom: {TransFrom}, transTo: {TransTo}", transFrom, transTo);
        return (transFrom, transTo, commission);
    }

    private decimal CalculateCommission(decimal amount, TransactionType transactionType)
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
}
