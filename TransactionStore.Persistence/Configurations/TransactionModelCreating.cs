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
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.AccountId).IsRequired();
        });
    }
}