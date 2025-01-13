using Microsoft.AspNetCore.Mvc;

namespace TransactionStore.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ILogger<TransactionController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{TransactionId}")]
        [EndpointSummary("Получение транзакции по Id")]
        public ActionResult GetTransactionById(Guid TransactionId)
        {
            return Ok();
        }

        [HttpPost]
        [EndpointSummary("Создание транзакции")]
        public IActionResult CreateTransaction(string currency)
        {
            return Ok(currency);
        }
    }
}
