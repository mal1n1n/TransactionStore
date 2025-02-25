using MYPBackendMicroserviceIntegrations.Enums;

namespace TransactionStore.Application.Models;
public class TransferTransactionModel
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public required Currency FromCurrency { get; set; }
    public required Currency ToCurrency { get; set; }
}
