using FinanceManagerBackend.API.Infrastructure;

namespace FinanceManagerBackend.API.Configuration;

public static class HostExtensions
{
    public static IHost CreateDbIfNotExists(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<Startup>>();

        try
        {
            var context = services.GetRequiredService<ServiceDbContext>();
            DbInitializer.Initialize(context, out bool isAlreadyInitialized);

            if (isAlreadyInitialized)
            {
                logger.LogInformation("Database is already initialized.");
            }
            else
            {
                logger.LogInformation("Database initialized successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred creating the Database.");
        }

        return host;
    }
}