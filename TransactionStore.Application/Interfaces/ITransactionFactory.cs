using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionFactory
{
    Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType);
    (Transaction transFrom, Transaction transTo) CreateTransferTransaction(TransferTransactionModel transferModel, decimal exchangeRate);
}