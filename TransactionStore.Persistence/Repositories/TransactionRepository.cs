using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Persistence.Repositories;
public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    public override async Task AddRangeAsync(IEnumerable<Transaction> entities)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await DbSet.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
