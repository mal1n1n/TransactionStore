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
            entity.Property(e => e.Amount).HasColumnType("decimal(8,2)");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp")
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.TransactionType).HasColumnType("smallint");
            // Индекс для AccountId
            entity.HasIndex(e => e.AccountId).HasDatabaseName("IX_Transaction_AccountId");

            // Составной индекс для AccountId и Date
            entity.HasIndex(e => new { e.AccountId, e.Date }).HasDatabaseName("IX_Transaction_AccountId_Date");

            // Составной индекс для Date и TransactionType
            entity.HasIndex(e => new { e.Date, e.TransactionType }).HasDatabaseName("IX_Transaction_Date_TransactionType");
        });
    }
}