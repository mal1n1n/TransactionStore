using TransactionStore.Application.Models;
using TransactionStore.Core;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionService
{
    Task<TransactionModel> GetTransactionByIdAsync(Guid id);
    Task<AccountBalanceModel> CountAccountBalance(Guid accountId);
    Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId);
    Task<Guid> CreateTransaction(TransactionModel transactionModel,TransactionType transactionType);
    Task<IEnumerable<Guid>> CreateTransferTransaction(TransferTransactionModel transferModel);
}
