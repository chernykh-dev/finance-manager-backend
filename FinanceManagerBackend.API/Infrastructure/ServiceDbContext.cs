using FinanceManagerBackend.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Infrastructure;

/// <summary>
/// App database context.
/// </summary>
public class ServiceDbContext(ILoggerFactory loggerFactory) : DbContext
{
    /// <summary>
    /// Accounts.
    /// </summary>
    public DbSet<Account> Accounts { get; set; }

    /// <summary>
    /// Currencies.
    /// </summary>
    public DbSet<Currency> Currencies { get; set; }

    /// <summary>
    /// Categories.
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Users.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Configure to localhost postgresql.
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "fmb-app";
        var dbUsername = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

        optionsBuilder
            .UseNpgsql($"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUsername};Password={dbPassword};")
            .UseLoggerFactory(loggerFactory)
            .UseSnakeCaseNamingConvention();
    }
}