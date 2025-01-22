using TransactionStore.Presentation.Configurations;
using TransactionStore.Persistence.Configurations;
using TransactionStore.Application.Configurations;

var builder = WebApplication.CreateBuilder(args);

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
