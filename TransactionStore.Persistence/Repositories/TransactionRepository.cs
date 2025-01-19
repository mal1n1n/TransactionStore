using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Persistence.Repositories;
public class TransactionRepository(
        ApplicationDbContext dbContext
    ) : BaseRepository<Transaction>(dbContext), ITransactionRepository
{
    public override async Task AddRangeAsync(IEnumerable<Transaction> entities)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await DbSet.AddRangeAsync(entities);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
