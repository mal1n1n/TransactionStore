using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TransactionStore.Application.Interfaces;
using TransactionStore.Persistence.Interfaces;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Domain.Controllers;

[ApiController]
[Route("api/v1/accounts")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly ITransactionService _transactionService;
    private readonly IMapper _mapper;
    public AccountController(ILogger<AccountController> logger, ITransactionService transactionService,IMapper mapper)
    {
        _logger = logger;
        _transactionService = transactionService;
        _mapper = mapper;
    }

    [HttpGet("{accountId}/balance")]
    [EndpointSummary("Получение баланса аккаунта")]
    public async Task<ActionResult<BalanceResponse>> GetBalance(Guid accountId)
    {
        var balance = await _transactionService.CountUserBalance(accountId);
        return Ok(new BalanceResponse());
    }

    [HttpGet("{accountId}/transactions")]
    [EndpointSummary("Получение транзакций аккаунта")]
    public async Task<ActionResult<List<ShortTransactionResponse>>> GetTransactions(Guid accountId)
    {
        var transactions = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
        var mappedTransactions = _mapper.Map<List<ShortTransactionResponse>>(transactions);
        return Ok(mappedTransactions);
    }
}
