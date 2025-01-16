using Microsoft.AspNetCore.Mvc;
using TransactionStore.Presentation.DTOs.Requests;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Presentation.Controllers;

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
    [EndpointDescription(
        "Передача currency в запросе необходима для того, чтобы вернулся текущий курс валюты<br>" +
        "Если currency не был указан, курс будет null")]
    public ActionResult<TransactionResponse> GetTransactionById(Guid transactionId,[FromQuery] string? currency)
    {
        var response = new TransactionResponse();
        return Ok(response);
    }

    [HttpPost("deposit")]
    [EndpointSummary("Создание транзакции на пополнение")]
    public ActionResult CreateDepositTransaction([FromBody] CreateDepositRequest request)
    {
        return Ok();
    }

    [HttpPost("withdraw")]
    [EndpointSummary("Создание транзакции на снятие средств")]
    public ActionResult CreateWithdrawalTransaction([FromBody] CreateWithdrawalRequest request)
    {
        return Ok();
    }

    [HttpPost("transfer")]
    [EndpointSummary("Создание трансферной транзакции")]
    public ActionResult CreateTransferTransaction([FromBody] CreateTransferRequest request)
    {
        return Ok();
    }
}
