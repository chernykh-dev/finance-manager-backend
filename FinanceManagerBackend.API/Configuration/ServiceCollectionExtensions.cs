using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.HttpPipelines;
using FinanceManagerBackend.API.Infrastructure;
using FinanceManagerBackend.API.Options;
using FinanceManagerBackend.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManagerBackend.API.Configuration;

/// <summary>
/// ServiceCollection extension methods.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure entity repositories to DI. Entities are taken from BaseEntity namespace (`Domain/Entities`).
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
    {
        var ns = typeof(BaseEntity).Namespace;

        var entityTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.Namespace == ns);

        foreach (var type in entityTypes)
        {
            var interfaceType = typeof(IEntityRepository<>).MakeGenericType(type);
            var implementationType = typeof(EntityRepository<>).MakeGenericType(type);

            services.AddScoped(interfaceType, implementationType);
        }

        return services;
    }

    /// <summary>
    /// Configure services to DI. Services are taken from IAuthService namespace (`Services`).
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        var ns = typeof(IAuthService).Namespace;

        var serviceTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.Namespace == ns)
            .ToList();

        var interfaceTypes = serviceTypes
            .Where(x => x.IsInterface);

        foreach (var interfaceType in interfaceTypes)
        {
            var implementationType = serviceTypes.FirstOrDefault(x => x.Name == interfaceType.Name[1..]);

            if (implementationType == null)
            {
                throw new NullReferenceException($"No implementation found for {interfaceType.FullName}.");
            }

            services.AddScoped(interfaceType, implementationType);
        }

        return services;
    }

    /// <summary>
    /// Configure authentication for app.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="authSection"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfigurationSection authSection)
    {
        var authOptions = authSection.Get<AuthOptions>();

        if (authOptions == null)
        {
            throw new NullReferenceException("Auth options are null.");
        }

        services.Configure<AuthOptions>(x =>
        {
            x.Issuer = authOptions.Issuer;
            x.Audience = authOptions.Audience;
            x.SecretKey = authOptions.SecretKey;
            x.ExpirationTimeInMinutes = authOptions.ExpirationTimeInMinutes;
        });

        services
            .AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    // Издатель.
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,

                    // Потребитель.
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = AuthHelper.GetSymmetricSecurityKey(authOptions.SecretKey),
                };

                // Disable automapping from sub to schema/.../nameidentifier
                opt.MapInboundClaims = false;
            });

        return services;
    }
}