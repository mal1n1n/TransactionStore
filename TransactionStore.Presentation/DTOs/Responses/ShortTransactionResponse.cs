using TransactionStore.Core;

namespace TransactionStore.Presentation.DTOs.Responses;
public class ShortTransactionResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
}
