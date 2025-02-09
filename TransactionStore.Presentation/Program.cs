using TransactionStore.Presentation.Configurations;
using TransactionStore.Persistence.Configurations;
using TransactionStore.Application.Configurations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration);
});
// Логирование строки подключения
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Log.Information("Connection string: {ConnectionString}", connectionString);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigurePersistence();  
builder.Services.ConfigurePresentationServices();
builder.Services.ConfigureApplication();  
builder.Services.ConfigureFluentValidators();
builder.Services.ConfigureMappers();
var app = builder.Build();
//app.UseIPFilter("127.0.0.1");
app.ConfigureCustomExceptionMiddleware();
app.ConfigurePresentation();
app.Run();
