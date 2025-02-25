using MYPBackendMicroserviceIntegrations.Enums;

namespace TransactionStore.Presentation.DTOs.Requests;
public class CreateTransferRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public required Currency FromCurrency { get; set; }
    public required Currency ToCurrency { get; set; }
}