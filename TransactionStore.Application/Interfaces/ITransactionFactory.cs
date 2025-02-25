using MYPBackendMicroserviceIntegrations.Enums;
using MYPBackendMicroserviceIntegrations.Messages;
using TransactionStore.Application.Models;
using TransactionStore.Persistence.Entities;

namespace TransactionStore.Application.Interfaces;
public interface ITransactionFactory
{
    Transaction CreateTransaction(TransactionModel transactionModel, TransactionType transactionType);
    TransactionCreatedMessage CreateTransactionMessage(Transaction transaction, Currency currency);
    TransactionCreatedMessage CreateTransferTransactionMessage(Transaction TransFrom, Transaction TransTo, TransferTransactionModel transferModel);
    TransferTransactionResult CreateTransferTransactions(TransferTransactionModel transferModel);
}