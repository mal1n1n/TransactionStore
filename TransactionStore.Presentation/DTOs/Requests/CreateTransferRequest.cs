using TransactionStore.Core;

namespace TransactionStore.Presentation.DTOs.Requests;
public class CreateTransferRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public required CurrencyType FromCurrency { get; set; }
    public required CurrencyType ToCurrency { get; set; }
}