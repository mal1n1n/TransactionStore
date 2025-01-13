namespace TransactionStore.Presentation.DTOs.Requests;
public class CreateWithdrawalRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }

}
