namespace TransactionStore.Presentation.DTOs.Requests;
public class CreateDepositRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}