using Microsoft.Extensions.Logging;
using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Persistence.Repositories;
public class TransactionRepository(
        ApplicationDbContext dbContext,
        ILogger<BaseRepository<Transaction>> logger
    ) : BaseRepository<Transaction>(dbContext, logger), ITransactionRepository
{
}
