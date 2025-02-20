using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Models;

public class TransferTransactionResult
{
    public Transaction TransFrom { get; set; }
    public Transaction TransTo { get; set; }
}
