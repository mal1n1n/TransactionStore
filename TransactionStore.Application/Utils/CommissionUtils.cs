using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TransactionStore.Application.Exceptions;
using TransactionStore.Application.Interfaces;
using TransactionStore.Core;

namespace TransactionStore.Application.Utils;

public class CommissionUtils(
        IConfiguration configuration,
        ILogger<CommissionUtils> logger) 
    : ICommissionUtils
{
    public decimal CalculateCommission(decimal amount, TransactionType transactionType)
    {
        string commissionKey = GetCommissionKey(transactionType);

        var commissionRate = configuration.GetValue<decimal?>(commissionKey);
        if (commissionRate == null || commissionRate.Value < 0)
        {
            logger.LogCritical($"{commissionKey} is not configured or negative");
            throw new CommissionSettingException($"{commissionKey} is not configured or went broken.");
        }
        return Math.Abs(amount) * (commissionRate.Value / 100);
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
