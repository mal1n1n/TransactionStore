using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Core;
using TransactionStore.Presentation.DTOs.Requests;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Presentation.Controllers;

[ApiController]
[Route("api/v1/transactions")]
public class TransactionController(
        ILogger<TransactionController> logger, 
        ITransactionService transactionService,
        IMapper mapper) 
    : ControllerBase
{
    [HttpGet("{transactionId}")]
    [EndpointSummary("Получение транзакции по Id")]
    public async Task<ActionResult<TransactionResponse>> GetTransactionById(Guid transactionId)
    {
        logger.LogInformation("GetTransactionById called with transactionId: {TransactionId}", transactionId);
        var transaction = await transactionService.GetTransactionByIdAsync(transactionId);
        var response = mapper.Map<TransactionResponse>(transaction);
        logger.LogInformation("GetTransactionById successful for transactionId: {TransactionId}", transactionId);
        return Ok(response);
    }

    [HttpPost("deposit")]
    [EndpointSummary("Создание транзакции на пополнение")]
    public async Task<ActionResult<Guid>> CreateDepositTransaction([FromBody] CreateTransactionRequest request)
    {
        logger.LogInformation("CreateDepositTransaction called with AccountId: {AccountId}, Amount: {Amount}", request.AccountId, request.Amount);
        var transaction = mapper.Map<TransactionModel>(request);
        transaction.TransactionType = TransactionType.Deposit;
        var response = await transactionService.CreateTransaction(transaction,TransactionType.Deposit);
        logger.LogInformation("CreateDepositTransaction successful for AccountId: {AccountId}, Amount: {Amount}", request.AccountId, request.Amount);
        return Ok(response);
    }

    [HttpPost("withdraw")]
    [EndpointSummary("Создание транзакции на снятие средств")]
    public async Task<ActionResult<Guid>> CreateWithdrawalTransaction([FromBody] CreateTransactionRequest request)
    {
        logger.LogInformation("CreateWithdrawalTransaction called with AccountId: {AccountId}, Amount: {Amount}", request.AccountId, request.Amount);
        var transaction = mapper.Map<TransactionModel>(request);
        transaction.TransactionType = TransactionType.Withdrawal;
        var response = await transactionService.CreateTransaction(transaction, TransactionType.Withdrawal);
        logger.LogInformation("CreateWithdrawalTransaction successful for AccountId: {AccountId}, Amount: {Amount}", request.AccountId, request.Amount);
        return Ok(response);
    }

    [HttpPost("transfer")]
    [EndpointSummary("Создание трансферной транзакции")]
    public async Task<ActionResult<IEnumerable<Guid>>> CreateTransferTransaction([FromBody] CreateTransferRequest request)
    {
        logger.LogInformation("CreateTransferTransaction called with FromAccountId: {FromAccountId}, ToAccountId: {ToAccountId}, Amount: {Amount}", request.FromAccountId, request.ToAccountId, request.Amount);
        var mappedTrans = mapper.Map<TransferTransactionModel>(request);
        var response = await transactionService.CreateTransferTransaction(mappedTrans);
        logger.LogInformation("CreateTransferTransaction successful for FromAccountId: {FromAccountId}, ToAccountId: {ToAccountId}, Amount: {Amount}", request.FromAccountId, request.ToAccountId, request.Amount);
        return Ok(response);
    }
}
