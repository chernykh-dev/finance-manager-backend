using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.UnitTests.CommonEntities;

public static class Users
{
    public static User SampleUser => new()
    {
        Id = Ids.SampleId,
        Name = "Sample User",
        Password = "Password"
    };
}