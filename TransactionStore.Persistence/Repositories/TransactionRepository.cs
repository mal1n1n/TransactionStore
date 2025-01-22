using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Persistence.Repositories;
public class TransactionRepository(
        ApplicationDbContext dbContext
    ) : BaseRepository<Transaction>(dbContext), ITransactionRepository
{
}
