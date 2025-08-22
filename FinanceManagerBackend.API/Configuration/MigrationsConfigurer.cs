using FinanceManagerBackend.API.Infrastructure;
using FinanceManagerBackend.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Configuration;

public static class MigrationsConfigurer
{
    public static void ApplyMigrations(this IApplicationBuilder application)
    {
        using var scope = application.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var pendingMigrations = context.Database.GetPendingMigrations();

        if (pendingMigrations.Any())
        {
            context.Database.Migrate();
        }
    }

}