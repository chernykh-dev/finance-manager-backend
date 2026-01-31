using FinanceManagerBackend.API.Configuration;

public class Program
{
    public static async Task Main(string[] args)
    {
        await Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder => { builder.UseStartup<Startup>(); })
            .Build()
            .MigrateDatabase()
            .SeedDatabase()
            .RunAsync();
    }
}
