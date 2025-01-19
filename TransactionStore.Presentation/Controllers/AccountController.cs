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
        var balance = await transactionService.CountUserBalance(accountId);
        var response = new BalanceResponse()
        {
            AccountId = accountId,
            Balance = balance
        };
        return Ok(response);
    }

    [HttpGet("{accountId}/transactions")]
    [EndpointSummary("Получение транзакций аккаунта")]
    public async Task<ActionResult<List<TransactionResponse>>> GetTransactions(Guid accountId)
    {
        var transactions = await transactionService.GetTransactionsByAccountIdAsync(accountId);
        var mappedTransactions = mapper.Map<List<TransactionResponse>>(transactions);
        return Ok(mappedTransactions);
    }
}
