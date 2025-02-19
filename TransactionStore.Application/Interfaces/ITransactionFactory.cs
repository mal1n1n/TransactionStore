using TransactionStore.Application.Messages;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionFactory
{
    Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType);
    TransactionCreatedMessage CreateTransactionMessage(Transaction transaction, CurrencyType currency);
    TransactionCreatedMessage CreateTransferTransactionMessage(Transaction TransFrom, Transaction TransTo, TransferTransactionModel transferModel);
    TransferTransactionResult CreateTransferTransactions(TransferTransactionModel transferModel);
}