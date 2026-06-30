using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models;
using Mapster;

namespace FinanceManagerBackend.API.Configuration;

/// <summary>
/// Mapster configuration.
/// </summary>
public static class MapsterConfiguration
{
    /// <summary>
    /// Register mappings.
    /// </summary>
    public static void RegisterMappings()
    {
        /*
        todo

        TypeAdapterConfig<BaseCreateRequest, BaseEntity>
            .NewConfig()
            .BeforeMapping((src, dest) =>
            {
                if (src.Id == Guid.Empty)
                    src.Id = Guid.NewGuid();
            });
        */
    }
}