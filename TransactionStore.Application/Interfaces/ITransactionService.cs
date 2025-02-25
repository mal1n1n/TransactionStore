using TransactionStore.Application.Models;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionService
{
    Task<TransactionModel> GetTransactionByIdAsync(Guid id);
    Task<AccountBalanceModel> CountAccountBalance(Guid accountId);
    Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId);
    Task<Guid> CreateDepositTransaction(TransactionModel transactionModel);
    Task<Guid> CreateWithdrawalTransaction(TransactionModel transactionModel);
    Task<IEnumerable<Guid>> CreateTransferTransaction(TransferTransactionModel transferModel);
}
