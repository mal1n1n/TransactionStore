namespace TransactionStore.Presentation.DTOs.Requests;
public class CreateTransferRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public required string FromCurrency { get; set; }
    public required string ToCurrency { get; set; }
}