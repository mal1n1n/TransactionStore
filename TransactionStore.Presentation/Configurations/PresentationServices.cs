using FluentValidation;
using FluentValidation.AspNetCore;
using TransactionStore.Presentation.DTOs.Requests.Validators;

namespace TransactionStore.Presentation.Configurations;
public static class PresentationServices
{
    public static void ConfigurePresentationServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
    public static void ConfigureFluentValidators(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<CreateTransactionRequestValidator>();
    }
}
