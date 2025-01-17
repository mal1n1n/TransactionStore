using TransactionStore.Application.Models;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionService
{
    Task<TransactionModel?> GetTransactionByIdAsync(Guid id);
    Task<decimal> CountUserBalance(Guid accountId);
    Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId);
    Task<TransactionModel> CreateTransaction(TransactionModel transactionModel);
    Task<IEnumerable<TransactionModel>> CreateTransferTransaction(IEnumerable<TransactionModel> transactionModels);
}
