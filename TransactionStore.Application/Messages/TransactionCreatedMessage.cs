using TransactionStore.Core;

namespace TransactionStore.Application.Messages;

public class TransactionCreatedMessage
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
}
