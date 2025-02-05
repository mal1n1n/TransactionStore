using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TransactionStore.Application.Interfaces;
using TransactionStore.Persistence.Interfaces;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Domain.Controllers;

[ApiController]
[Route("api/v1/accounts")]
public class AccountController(
        ILogger<AccountController> logger, 
        ITransactionService transactionService, 
        IMapper mapper) : 
    ControllerBase
{
    [HttpGet("{accountId}/balance")]
    [EndpointSummary("Получение баланса аккаунта")]
    public async Task<ActionResult<BalanceResponse>> GetBalance(Guid accountId)
    {
        logger.LogInformation("GetBalance called with accountId: {AccountId}", accountId);
        var balance = await transactionService.CountAccountBalance(accountId);
        var mappedBalance = mapper.Map<BalanceResponse>(balance);
        logger.LogInformation("GetBalance successful for accountId: {AccountId} with balance {Balance}", accountId,mappedBalance.Balance);
        return Ok(mappedBalance);
    }

    [HttpGet("{accountId}/transactions")]
    [EndpointSummary("Получение транзакций аккаунта")]
    public async Task<ActionResult<List<TransactionResponse>>> GetTransactions(Guid accountId)
    {
        logger.LogInformation("GetTransactions called with accountId: {AccountId}", accountId);
        var transactions = await transactionService.GetTransactionsByAccountIdAsync(accountId);
        var mappedTransactions = mapper.Map<List<TransactionResponse>>(transactions);
        logger.LogInformation("GetTransactions successful for accountId: {AccountId} with {TransactionsCount} transactions", accountId, mappedTransactions.Count);
        return Ok(mappedTransactions);
    }
}
