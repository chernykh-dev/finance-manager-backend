using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Infrastructure;

/// <summary>
/// Database initializer.
/// </summary>
public class DbInitializer
{
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
                new Currency() { Id = Guid.NewGuid(), Name = "RUB" },
                new Currency() { Id = Guid.NewGuid(), Name = "USD" },
                new Currency() { Id = Guid.NewGuid(), Name = "EUR" }
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
                new Category() { Id = Guid.NewGuid(), Name = "Зарплата", Emoji = "💰", IsIncome = true },
                new Category() { Id = Guid.NewGuid(), Name = "Фриланс", Emoji = "💻", IsIncome = true },
                new Category() { Id = Guid.NewGuid(), Name = "Подарки", Emoji = "🎁", IsIncome = true },
                new Category() { Id = Guid.NewGuid(), Name = "Проценты по вкладам", Emoji = "🏦", IsIncome = true },
                new Category() { Id = Guid.NewGuid(), Name = "Возврат долга", Emoji = "🔄", IsIncome = true },
                new Category() { Id = Guid.NewGuid(), Name = "Продажа имущества", Emoji = "🏠", IsIncome = true },

                new Category() { Id = Guid.NewGuid(), Name = "Жильё", Emoji = "🏠", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Продукты", Emoji = "🍎", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Транспорт", Emoji = "🚗", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Развлечения", Emoji = "🎭", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Рестораны", Emoji = "🍽️", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Одежда", Emoji = "👕", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Здоровье", Emoji = "🏥", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Коммунальные услуги", Emoji = "💡", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Техника", Emoji = "📱", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Образование", Emoji = "📚", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Путешествия", Emoji = "✈️", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Подписки", Emoji = "📺", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Подарки", Emoji = "🎀", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Красота", Emoji = "💄", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Спорт", Emoji = "🏋️", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Домашние животные", Emoji = "🐾", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Хобби", Emoji = "🎨", IsIncome = false },
                new Category() { Id = Guid.NewGuid(), Name = "Кредиты", Emoji = "💳", IsIncome = false },
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