using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Configuration.MapsterReflection;
using Mapster;

namespace FinanceManagerBackend.API.Configuration;

public class MapsterRegister : ICodeGenerationRegister
{
    private Type[] _entityTypes;

    public void Register(CodeGenerationConfig config)
    {
        _entityTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.BaseType == typeof(BaseEntity))
            .ToArray();

        RegisterCreateRequestModels(config);

        RegisterUpdateRequestModels(config);

        RegisterResponseModels(config);
    }

    private void RegisterCreateRequestModels(CodeGenerationConfig config)
    {
        var builder = config.AdaptTo("[name]CreateRequest")
            .ForTypes(_entityTypes);

        foreach (var type in _entityTypes)
        {
            builder.MakePropertyNullable(type, nameof(BaseEntity.Id));
            builder.IgnoreProperty(type, nameof(BaseEntity.CreatedAt));
            builder.IgnoreProperty(type, nameof(BaseEntity.UpdatedAt));
        }
    }

    private void RegisterUpdateRequestModels(CodeGenerationConfig config)
    {
        var builder = config.AdaptTo("[name]UpdateRequest")
            .ForTypes(_entityTypes);

        foreach (var type in _entityTypes)
        {
            builder.IgnoreProperty(type, nameof(BaseEntity.Id));
            builder.MakePropertyNullable(type, nameof(BaseEntity.CreatedAt));
            builder.MakePropertyNullable(type, nameof(BaseEntity.UpdatedAt));
        }
    }

    private void RegisterResponseModels(CodeGenerationConfig config)
    {
        config.AdaptTo("[name]Response")
            .ForTypes(_entityTypes);
    }
}