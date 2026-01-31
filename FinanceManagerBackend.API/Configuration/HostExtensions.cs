using FinanceManagerBackend.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Configuration;

/// <summary>
/// Host extension methods.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Apply migrations to database.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Seed database initial data.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static IHost SeedDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<Startup>>();

        try
        {
            var context = services.GetRequiredService<ServiceDbContext>();
            DbInitializer.Initialize(context, out var isAlreadyInitialized);

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