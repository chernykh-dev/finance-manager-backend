using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.UnitTests.CommonEntities;

public static class Categories
{
    public static Category SampleCategory => new ()
    {
        Id = Ids.SampleId,
        Name = "Sample Category",
        Emoji = " ",
        IsIncome = false
    };

    public static Category SampleIncomeCategory => new ()
    {
        Id = Ids.SampleId2,
        Name = "Sample Income Category",
        Emoji = " ",
        IsIncome = true
    };
}