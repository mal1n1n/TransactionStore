namespace TransactionStore.Presentation.Configurations;

public static class ApplicationConfiguration
{
    public static void ConfigureApplication(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
