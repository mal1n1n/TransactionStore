using Microsoft.EntityFrameworkCore;
using TransactionStore.Persistence.Configurations;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Persistence.Data;
public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options
    ) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureTransactionModelCreating();
        base.OnModelCreating(modelBuilder);
    }
}