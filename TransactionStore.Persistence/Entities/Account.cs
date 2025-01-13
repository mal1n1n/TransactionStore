using TransactionStore.Core;

namespace TransactionStore.Persistence.Entities;
public class Account
{
    public Guid Id { get; set; }
    public required Currencies Currency { get; set; }
    public decimal Balance { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = [];
}
