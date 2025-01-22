using TransactionStore.Core;
namespace TransactionStore.Application.Models;
public class TransferTransactionModel
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public required CurrencyType FromCurrency { get; set; }
    public required CurrencyType ToCurrency { get; set; }
}
