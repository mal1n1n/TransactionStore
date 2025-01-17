using TransactionStore.Core;

namespace TransactionStore.Presentation.DTOs.Responses;
public class TransactionResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
}
