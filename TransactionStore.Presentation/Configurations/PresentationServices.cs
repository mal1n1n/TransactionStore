using FluentValidation;
using FluentValidation.AspNetCore;
using TransactionStore.Presentation.DTOs.Requests.Validators;
using TransactionStore.Presentation.Models;

namespace TransactionStore.Presentation.Configurations;
public static class PresentationServices
{
    public static void ConfigurePresentationServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<AllowedIpsOptions>(configuration.GetSection("AllowedIpsOptions"));
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
