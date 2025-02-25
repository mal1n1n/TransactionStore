using MYPBackendMicroserviceIntegrations.Enums;
namespace TransactionStore.Application.Models;
public class TransactionModel
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionModel? RelatedTransaction { get; set; }
}
