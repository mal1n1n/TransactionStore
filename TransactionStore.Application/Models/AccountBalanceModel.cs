namespace TransactionStore.Application.Models;
public class AccountBalanceModel
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
}
