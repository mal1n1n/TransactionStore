using TransactionStore.Persistence.Entities;

namespace TransactionStore.Persistence.Interfaces;
public interface ITransactionRepository : IBaseRepository<Transaction>
{
    Task<Guid> CreateTransactionAsync(Transaction transaction, Guid? lastTransactionId);
    Task<(Guid fromTransactionId, Guid toTransactionId)> CreateTransferTransactionAsync(Transaction transFrom, Transaction transTo, Guid? lastTransactionId);
}