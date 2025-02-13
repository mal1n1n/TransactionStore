namespace TransactionStore.Application.Exceptions;
public class InsufficientFundsException(string message) : Exception(message)
{
}
