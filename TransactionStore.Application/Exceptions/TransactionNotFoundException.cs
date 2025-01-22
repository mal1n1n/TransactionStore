namespace TransactionStore.Application.Exceptions;
public class TransactionNotFoundException(string message) : Exception(message)
{
}
