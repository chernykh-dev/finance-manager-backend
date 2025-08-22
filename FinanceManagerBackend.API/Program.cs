using FinanceManagerBackend.API.Configuration;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder =>
    {
        builder.UseStartup<Startup>();
    })
    .Build()
    .CreateDbIfNotExists()
    .RunAsync();
