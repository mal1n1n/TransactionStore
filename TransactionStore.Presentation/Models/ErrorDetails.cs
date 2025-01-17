using System.Text.Json;

namespace TransactionStore.Presentation.Models;
public class ErrorDetails
{
    public int StatusCode { get; set; }
    public required string Message { get; set; }
    public override string ToString() => JsonSerializer.Serialize(this);
}
