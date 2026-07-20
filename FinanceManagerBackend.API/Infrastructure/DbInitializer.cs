using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Infrastructure;

/// <summary>
/// Database initializer.
/// </summary>
public class DbInitializer
{
    /// <summary>
    /// Run currency id.
    /// </summary>
    public const string RubCurrencyId = "d67bdffe-9f2d-45e4-809a-c566f537dfb7";

    /// <summary>
    /// Usd currency id.
    /// </summary>
    public const string UsdCurrencyId = "0fbe774f-42fd-4814-b3c2-6ea659b4e595";

    /// <summary>
    /// Eur currency id.
    /// </summary>
    public const string EurCurrencyId = "e17d279c-4ac8-4679-894d-5b5f5caed741";

    /// <summary>
    /// Initialize database required data.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="isAlreadyInitialized"></param>
    public static void Initialize(ServiceDbContext context, out bool isAlreadyInitialized)
    {
        context.Database.EnsureCreated();

        isAlreadyInitialized = true;

        if (!context.Currencies.Any())
        {
            var currencies = new List<Currency>()
            {
                new Currency() { Id = Guid.Parse(RubCurrencyId), Code = "RUB", Symbol = "₽" },
                new Currency() { Id = Guid.Parse(UsdCurrencyId), Code = "USD", Symbol = "$" },
                new Currency() { Id = Guid.Parse(EurCurrencyId), Code = "EUR", Symbol = "€" }
            };

            foreach (var currency in currencies)
            {
                context.Currencies.Add(currency);
            }

            isAlreadyInitialized = false;
        }

        if (!context.Categories.Any())
        {
            var categories = new List<Category>()
            {
                new Category() { Id = Guid.Parse("8ec25ef1-92e9-4267-9f93-5cef7a206dc2"), Name = "Зарплата", Emoji = "💰", IsIncome = true },
                new Category() { Id = Guid.Parse("c228b14c-2060-4115-a3ce-1ac30e741234"), Name = "Фриланс", Emoji = "💻", IsIncome = true },
                new Category() { Id = Guid.Parse("089be429-4aa6-40a1-aab7-7fc763ff4932"), Name = "Подарки", Emoji = "🎁", IsIncome = true },
                new Category() { Id = Guid.Parse("962c2e56-d372-41fe-942c-b4e7b562bdc0"), Name = "Проценты по вкладам", Emoji = "🏦", IsIncome = true },
                new Category() { Id = Guid.Parse("68e198c6-4759-4051-ac48-7630e2e7db98"), Name = "Возврат долга", Emoji = "🔄", IsIncome = true },
                new Category() { Id = Guid.Parse("ce8a6ed9-6aa2-4a1b-8676-85d97466b43d"), Name = "Продажа имущества", Emoji = "🏠", IsIncome = true },

                new Category() { Id = Guid.Parse("794db1dc-c4cd-4836-85d1-71483c197db9"), Name = "Жильё", Emoji = "🏠", IsIncome = false },
                new Category() { Id = Guid.Parse("99bd7e92-b0b2-428f-b461-7400ecf509d8"), Name = "Продукты", Emoji = "🍎", IsIncome = false },
                new Category() { Id = Guid.Parse("e24f5cba-2fa9-4fa1-b4c5-b9140e42655c"), Name = "Транспорт", Emoji = "🚗", IsIncome = false },
                new Category() { Id = Guid.Parse("fe2e0257-7b19-4194-b412-8cd253d446ec"), Name = "Развлечения", Emoji = "🎭", IsIncome = false },
                new Category() { Id = Guid.Parse("909cc325-a4f6-44ef-9b55-7b7c3b60ef86"), Name = "Рестораны", Emoji = "🍽️", IsIncome = false },
                new Category() { Id = Guid.Parse("c283d146-b4c3-4d72-a8dc-6c835ccee030"), Name = "Одежда", Emoji = "👕", IsIncome = false },
                new Category() { Id = Guid.Parse("705c9b3e-e2de-459b-b78b-7089fd0ea915"), Name = "Здоровье", Emoji = "🏥", IsIncome = false },
                new Category() { Id = Guid.Parse("10f12527-5011-4237-9292-e86fbf334700"), Name = "Коммунальные услуги", Emoji = "💡", IsIncome = false },
                new Category() { Id = Guid.Parse("587cd29e-fb78-4bdb-818b-fa16fc018f1c"), Name = "Техника", Emoji = "📱", IsIncome = false },
                new Category() { Id = Guid.Parse("00ef1842-5d6c-4a47-a57a-39147de8af33"), Name = "Образование", Emoji = "📚", IsIncome = false },
                new Category() { Id = Guid.Parse("2aa08597-21ec-43f7-80f7-7da9acfda5b6"), Name = "Путешествия", Emoji = "✈️", IsIncome = false },
                new Category() { Id = Guid.Parse("df041b86-578e-4e0c-aa32-2c63e6015185"), Name = "Подписки", Emoji = "📺", IsIncome = false },
                new Category() { Id = Guid.Parse("fa640daa-cb85-43c4-89b7-8f518a8f8088"), Name = "Подарки", Emoji = "🎀", IsIncome = false },
                new Category() { Id = Guid.Parse("97cc434d-7c1e-4900-a134-3ef4845c4d8f"), Name = "Красота", Emoji = "💄", IsIncome = false },
                new Category() { Id = Guid.Parse("d9cdf036-2c80-449b-babf-48f9d8dc5726"), Name = "Спорт", Emoji = "🏋️", IsIncome = false },
                new Category() { Id = Guid.Parse("fa67619c-b8c9-4b87-ae03-0650b62ae219"), Name = "Домашние животные", Emoji = "🐾", IsIncome = false },
                new Category() { Id = Guid.Parse("cf22d158-fd05-4a8a-800d-8938a34319a9"), Name = "Хобби", Emoji = "🎨", IsIncome = false },
                new Category() { Id = Guid.Parse("8d571312-2530-472b-8c62-7329103b15dd"), Name = "Кредиты", Emoji = "💳", IsIncome = false },
            };

            foreach (var category in categories)
            {
                context.Categories.Add(category);
            }

            isAlreadyInitialized = false;
        }

        if (!isAlreadyInitialized)
        {
            context.SaveChanges();
        }
    }
}