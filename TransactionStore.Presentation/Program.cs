using TransactionStore.Presentation.Configurations;
using TransactionStore.Persistence.Configurations;
using TransactionStore.Application.Configurations;
using Serilog;
using TransactionStore.Presentation.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration);
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigurePresentationServices(builder.Configuration);
builder.Services.ConfigurePersistence(builder.Configuration);  
builder.Services.ConfigureApplication(builder.Configuration);  
builder.Services.ConfigureFluentValidators();
builder.Services.ConfigureMappers();



var app = builder.Build();

app.UseIPFilter();
app.ConfigureCustomExceptionMiddleware();
app.ConfigurePresentation();
app.Run();
