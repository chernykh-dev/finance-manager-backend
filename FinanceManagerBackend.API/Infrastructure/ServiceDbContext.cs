using FinanceManagerBackend.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Infrastructure;

public class ServiceDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    public DbSet<Currency> Currencies { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5432;Database=fmb-db;Username=postgres;Password=postgres;")
            .LogTo(Console.WriteLine)
            .UseSnakeCaseNamingConvention();
    }
}