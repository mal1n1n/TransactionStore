using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Persistence.Repositories;
public class TransactionRepository(
    ApplicationDbContext dbContext
    ) : BaseRepository<Account>(dbContext)
{

}
