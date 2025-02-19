using TransactionStore.Core;

namespace TransactionStore.Persistence.Entities;
public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }

    public Transaction(Guid accountId, decimal amount, TransactionType transactionType)
    {
        AccountId = accountId;
        Amount = amount;
        TransactionType = transactionType;
    }
    public Transaction() { }
}
