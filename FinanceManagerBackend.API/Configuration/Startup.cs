using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FinanceManagerBackend.API.Exceptions;
using FinanceManagerBackend.API.HttpPipelines.ExceptionHandlers;
using FinanceManagerBackend.API.Infrastructure;
using FinanceManagerBackend.API.Validators;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace FinanceManagerBackend.API.Configuration;

/// <summary>
/// Startup configurations.
/// </summary>
public class Startup
{
    /// <summary>
    /// App configuration.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="env"></param>
    public Startup(IWebHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

        if (env.IsEnvironment("Development"))
        {
            // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
            // builder.AddApplicationInsightsSettings(developerMode: true);
        }

        builder.AddEnvironmentVariables();
        Configuration = builder.Build();
    }

    /// <summary>
    /// Configure services for app.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        services.AddDbContext<ServiceDbContext>();

        services.ConfigureRepositories();
        services.ConfigureServices();
        services.ConfigureExceptionHandlers();
        services.AddProblemDetails(opt =>
        {
            opt.CustomizeProblemDetails = context =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";

                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        services.AddMapster();
        services.AddValidatorsFromAssemblyContaining(typeof(AccountRelationsValidator));

        services.AddSwaggerGen(opt =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            opt.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        services.AddControllers(opt =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(JwtRegisteredClaimNames.Sub)
                .Build();

            opt.Filters.Add(new AuthorizeFilter(policy));
        });

        services.AddEndpointsApiExplorer();

        services
            .AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1);
                opt.ReportApiVersions = true;
                opt.AssumeDefaultVersionWhenUnspecified = true;

                opt.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(opt =>
            {
                opt.GroupNameFormat = "'v'VVV";
                opt.SubstituteApiVersionInUrl = true;
            });

        services.ConfigureAuthentication(Configuration.GetSection("Auth"));

        services.ConfigureOptions<ConfigureSwaggerOptions>();
        services.Configure<RouteOptions>(opt =>
        {
            opt.LowercaseUrls = true;
        });
    }

    /// <summary>
    /// Configure app.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseExceptionHandler();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.DocumentTitle = "Finance Manager Backend API";

                opt.EnableTryItOutByDefault();
                // opt.EnablePersistAuthorization();

                opt.DisplayRequestDuration();
                opt.CacheLifetime = null;

                var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToLowerInvariant());
                }
            });
        }

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // app.UseStatusCodePages();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}