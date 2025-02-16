using TransactionStore.Core;

namespace TransactionStore.Application.Messages;

public class TransactionCreatedMessage
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountRub { get; set; }
    public decimal Commission { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionCreatedMessage? RelatedTransaction { get; set; }
}
