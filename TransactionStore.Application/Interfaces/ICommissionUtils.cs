using TransactionStore.Core;

namespace TransactionStore.Application.Interfaces
{
    public interface ICommissionUtils
    {
        decimal CalculateCommission(decimal amount, TransactionType transactionType);
    }
}