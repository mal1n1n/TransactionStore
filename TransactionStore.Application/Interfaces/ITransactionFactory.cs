using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionFactory
{
    Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType, out decimal commission);
    (Transaction transFrom, Transaction transTo, decimal commission) CreateTransferTransaction(TransferTransactionModel transferModel, decimal exchangeRate);
}