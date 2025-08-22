using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinanceManagerBackend.API.Configuration;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateVersionInfo(description));
        }
    }

    public void Configure(string name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = $"Finance Manager Backend API",
            Version = description.ApiVersion.ToString(),
            Description = "This API provides endpoints for managing and retrieving data in a secure and efficient way. " +
                          "It supports CRUD operations, filtering, and pagination, and follows RESTful principles. " +
                          "Use this API to integrate with our system, automate workflows, and access resources programmatically."
        };

        if (description.IsDeprecated)
        {
            info.Title += " [Deprecated]";
        }

        return info;
    }
}