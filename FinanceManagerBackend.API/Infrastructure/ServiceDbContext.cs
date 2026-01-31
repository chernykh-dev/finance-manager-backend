using FinanceManagerBackend.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Infrastructure;

/// <summary>
/// App database context.
/// </summary>
public class ServiceDbContext(ILogger<ServiceDbContext> logger) : DbContext
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
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";

        logger.LogInformation(dbHost);

        optionsBuilder
            .UseNpgsql($"Host={dbHost};Port=5432;Database=fmb-db;Username=postgres;Password=postgres;")
            .LogTo(Console.WriteLine)
            .UseSnakeCaseNamingConvention();
    }
}