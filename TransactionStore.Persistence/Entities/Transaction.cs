namespace TransactionStore.Persistence.Entities;
public class Transaction
{
    public Guid Id { get; set; }
    public required Account Account { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

}
