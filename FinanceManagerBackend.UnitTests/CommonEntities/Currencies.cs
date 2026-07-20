using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.UnitTests.CommonEntities;

public static class Currencies
{
    public static Currency RubCurrency => new()
    {
        Id = Ids.SampleId,
        Code = "RUB"
    };
}