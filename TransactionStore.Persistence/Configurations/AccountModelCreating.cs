using Microsoft.EntityFrameworkCore;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Persistence.Configurations;
internal static class AccountModelCreating
{
    internal static void ConfigureAccountModelCreating(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Currency)
                .IsRequired();
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18,2)");
            entity.HasMany(e => e.Transactions)
                .WithOne()
                .HasForeignKey(e => e.AccountId)
                .HasPrincipalKey(e => e.Id);
        });
    }
}
