using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.UnitTests.CommonEntities;

public static class Accounts
{
    public static Account SampleAccount => new()
    {
        Id = Ids.SampleId,
        Name = "Sample Account",
        CurrencyId = Currencies.RubCurrency.Id,
        UserId = Users.SampleUser.Id,
        Balance = 0
    };

    public static Account SampleAccount2 => new()
    {
        Id = Ids.SampleId2,
        Name = "Another Sample Account",
        CurrencyId = Currencies.RubCurrency.Id,
        UserId = Users.SampleUser.Id,
        Balance = 0
    };
}