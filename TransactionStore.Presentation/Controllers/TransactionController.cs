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
        var transaction = await transactionService.GetTransactionByIdAsync(transactionId);
        var response = mapper.Map<TransactionResponse>(transaction);
        return Ok(response);
    }

    [HttpPost("deposit")]
    [EndpointSummary("Создание транзакции на пополнение")]
    public async Task<ActionResult<Guid>> CreateDepositTransaction([FromBody] CreateTransactionRequest request)
    {
        var transaction = mapper.Map<TransactionModel>(request);
        transaction.TransactionType = TransactionType.Deposit;
        var response = await transactionService.CreateTransaction(transaction);
        return Ok(response);
    }

    [HttpPost("withdraw")]
    [EndpointSummary("Создание транзакции на снятие средств")]
    public async Task<ActionResult<Guid>> CreateWithdrawalTransaction([FromBody] CreateTransactionRequest request)
    {
        var transaction = mapper.Map<TransactionModel>(request);
        transaction.TransactionType = TransactionType.Withdrawal;
        var response = await transactionService.CreateTransaction(transaction);
        return Ok(response);
    }

    [HttpPost("transfer")]
    [EndpointSummary("Создание трансферной транзакции")]
    public async Task<ActionResult<IEnumerable<Guid>>> CreateTransferTransaction([FromBody] CreateTransferRequest request)
    {
        var mappedTrans = mapper.Map<TransferTransactionModel>(request);
        var response = await transactionService.CreateTransferTransaction(mappedTrans);
        return Ok();
    }
}
