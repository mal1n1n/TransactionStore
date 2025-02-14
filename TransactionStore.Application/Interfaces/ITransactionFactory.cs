using TransactionStore.Application.Messages;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionFactory
{
    decimal CalculateCommission(decimal amount, TransactionType transactionType);
    Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType, decimal commission);
    TransactionCreatedMessage CreateTransactionMessage(Transaction transaction, CurrencyType currency, decimal commission);
    TransferTransactionResult CreateTransferTransactionMessages(TransferTransactionResult transferResult, TransferTransactionModel transferModel, decimal commission);
    TransferTransactionResult CreateTransferTransactions(TransferTransactionModel transferModel, decimal exchangeRate, decimal commission);
}