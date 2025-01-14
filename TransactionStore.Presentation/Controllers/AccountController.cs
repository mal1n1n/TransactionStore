using Microsoft.AspNetCore.Mvc;

namespace TransactionStore.Domain.Controllers
{
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
        public ActionResult<decimal> GetBalance(Guid accountId)
        {
            var rnd = new Random();
            var value = rnd.NextDouble() * 3000 - 1000;
            _logger.Log(LogLevel.Information, "Returned balance {Value} for {AccountId}", value, accountId);
            return Ok((decimal)value);
        }

        [HttpGet("{accountId}/transactions")]
        [EndpointSummary("Получение транзакций аккаунта")]
        public ActionResult GetTransactions(Guid accountId)
        {
            return Ok();
        }
    }
}
