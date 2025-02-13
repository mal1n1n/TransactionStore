using System.Net;
using TransactionStore.Application.Exceptions;
using TransactionStore.Presentation.Models;

namespace TransactionStore.Presentation.Middlewares;
public class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            httpContext.Response.ContentType = "application/json";
            await next(httpContext);
        }
        catch (InvalidExchangeRateException ex)
        {
            await HandleInvalidExchangeRateException(httpContext, ex);
        }
        catch (TransactionNotFoundException ex)
        {
            await HandleTransactionNotFoundException(httpContext, ex);
        }
        catch (InsufficientFundsException ex)
        {
            await HandleInsufficientFundsException(httpContext, ex);
        }
        catch (CommissionSettingException ex)
        {
            await HandleCommisionSettingException(httpContext, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    private async Task HandleCommisionSettingException(HttpContext context, Exception exception)
    {
        await HandleExceptionAsync(context, exception, HttpStatusCode.InternalServerError, "Commission setting is not configured or went broken.");
    }
    private async Task HandleTransactionNotFoundException(HttpContext context, Exception exception)
    {
        await HandleExceptionAsync(context, exception, HttpStatusCode.NotFound, "Transaction not found.");
    }

    private async Task HandleInvalidExchangeRateException(HttpContext context, Exception exception)
    {
        await HandleExceptionAsync(context, exception, HttpStatusCode.InternalServerError, "Invalid exchange rate.");
    }

    private async Task HandleInsufficientFundsException(HttpContext context, Exception exception)
    {
        await HandleExceptionAsync(context, exception, HttpStatusCode.PaymentRequired, "Insufficient funds.");
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        await HandleExceptionAsync(context, exception, HttpStatusCode.InternalServerError, "Internal Server Error from the custom middleware.");
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = message
        }.ToString());
    }
}
