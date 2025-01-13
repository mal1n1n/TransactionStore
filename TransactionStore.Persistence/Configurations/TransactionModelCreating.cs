using Microsoft.EntityFrameworkCore;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Persistence.Configurations;
internal static class TransactionModelCreating
{
    internal static void ConfigureTransactionModelCreating(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.HasOne(e => e.Account)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.AccountId)
                .HasPrincipalKey(e => e.Id);
        });
    }
}