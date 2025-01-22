using Microsoft.Extensions.DependencyInjection;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;
using TransactionStore.Persistence.Repositories;

namespace TransactionStore.Persistence.Configurations;
public static class PersistenceConfiguration
{
    public static void ConfigurePersistence(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
    }
}
