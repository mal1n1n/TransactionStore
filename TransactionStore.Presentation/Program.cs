using TransactionStore.Presentation.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureFluentValidators();

var app = builder.Build();
app.ConfigureCustomExceptionMiddleware();
app.ConfigureApplication();
app.Run();
