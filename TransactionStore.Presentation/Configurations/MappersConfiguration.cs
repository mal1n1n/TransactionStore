using TransactionStore.Application.Mappings;
using TransactionStore.Presentation.Mappings;

namespace TransactionStore.Presentation.Configurations;
public static class MappersConfiguration
{
    public static void ConfigureMappers(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(TransactionMapperProfile).Assembly);
        services.AddAutoMapper(typeof(TransactionDtoMapperProfile).Assembly);
    }
}
