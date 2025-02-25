using TransactionStore.Persistence.Entities;

namespace TransactionStore.Persistence.Interfaces;
public interface ITransactionRepository : IBaseRepository<Transaction>
{
    Task CreateWithdrawalTransactionAsync(Transaction transaction, Guid? lastTransactionId);
    Task CreateTransferTransactionAsync(Transaction transFrom, Transaction transTo, Guid? lastTransactionId);
}