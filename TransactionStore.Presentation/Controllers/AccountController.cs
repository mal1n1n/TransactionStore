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

        [HttpGet("{AccountId}/balance")]
        [EndpointSummary("Получение баланса аккаунта")]
        public ActionResult<decimal> GetBalance(Guid AccountId)
        {
            var rnd = new Random();
            var value = rnd.NextDouble() * 3000 - 1000;
            _logger.Log(LogLevel.Information, "Returned balance {Value} for {AccountId}", value, AccountId);
            return Ok((decimal)value);
        }

        [HttpGet("{AccountId}/transactions")]
        [EndpointSummary("Получение транзакций аккаунта")]
        public ActionResult GetTransactions(Guid AccountId)
        {
            return Ok();
        }

        [HttpPost]
        [EndpointSummary("Создание аккаунта")]
        public IActionResult CreateAccount(string currency)
        {
            return Ok(currency);
        }
    }
}
