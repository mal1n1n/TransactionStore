using TransactionStore.Core;

namespace TransactionStore.Application.Interfaces
{
    public interface ICommissionUtil
    {
        decimal CalculateCommission(decimal amount, TransactionType transactionType);
    }
}