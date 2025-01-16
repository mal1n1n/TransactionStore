using Microsoft.AspNetCore.Mvc;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Domain.Controllers;

[ApiController]
[Route("api/v1/accounts")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{accountId}/balance")]
    [EndpointSummary("Получение баланса аккаунта")]
    public ActionResult<BalanceResponse> GetBalance(Guid accountId)
    {
        return Ok(new BalanceResponse());
    }

    [HttpGet("{accountId}/transactions")]
    [EndpointSummary("Получение транзакций аккаунта")]
    public ActionResult<List<ShortTransactionResponse>> GetTransactions(Guid accountId)
    {
        return Ok(new List<ShortTransactionResponse>() { new ShortTransactionResponse()});
    }
}
