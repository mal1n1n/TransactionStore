using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using TransactionStore.Persistence.Interfaces;
using TransactionStore.Persistence.Repositories;

namespace TransactionStore.Persistence.Configurations;
public static class PersistenceConfiguration
{
    public static void ConfigurePersistence(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IDbConnection>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            return new NpgsqlConnection(connectionString);
        });
    }
}
