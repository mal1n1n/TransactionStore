using TransactionStore.Core;

namespace TransactionStore.Presentation.DTOs.Requests;
public class CreateTransactionRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public required CurrencyType Currency { get; set; }
}