using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Persistence.Repositories;
public class AccountRepository(
    ApplicationDbContext dbContext
    ) : BaseRepository<Account>(dbContext)
{

}
