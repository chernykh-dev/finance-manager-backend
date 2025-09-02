using FinanceManagerBackend.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Configuration;

public static class HostExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var pendingMigrations = context.Database.GetPendingMigrations();

        if (pendingMigrations.Any())
        {
            context.Database.Migrate();
        }

        return host;
    }

    public static IHost SeedDatabase(this IHost host)
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