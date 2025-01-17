namespace TransactionStore.Presentation.Configurations;

public static class PresentationConfiguration
{
    public static void ConfigurePresentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
