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
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigurePersistence();  
builder.Services.ConfigurePresentationServices();
builder.Services.ConfigureApplication(builder.Configuration);  
builder.Services.ConfigureFluentValidators();
builder.Services.ConfigureMappers();
var app = builder.Build();
//app.UseIPFilter("127.0.0.1");
app.ConfigureCustomExceptionMiddleware();
app.ConfigurePresentation();
app.Run();
