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

        [HttpGet("{transactionId}")]
        [EndpointSummary("Получение транзакции по Id")]
        public ActionResult GetTransactionById(Guid transactionId)
        {
            return Ok();
        }

        [HttpPost("deposit")]
        [EndpointSummary("Создание транзакции на пополнение")]
        public ActionResult CreateDepositTransaction()
        {
            return Ok();
        }

        [HttpPost("withdraw")]
        [EndpointSummary("Создание транзакции на снятие средств")]
        public ActionResult CreateWithdrawalTransaction()
        {
            return Ok();
        }

        [HttpPost("transfer")]
        [EndpointSummary("Создание трансферной транзакции")]
        public ActionResult CreateTransferTransaction()
        {
            return Ok();
        }
    }
}
