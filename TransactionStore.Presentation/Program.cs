using TransactionStore.Presentation.Configurations;
using TransactionStore.Persistence.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigurePresentationServices();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureFluentValidators();

var app = builder.Build();
//app.UseIPFilter("127.0.0.1");
app.ConfigureCustomExceptionMiddleware();
app.ConfigurePresentation();
app.Run();
