using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FluentValidation;

namespace FinanceManagerBackend.API.Validators;

/// <summary>
/// Validator for check account foreign keys.
/// </summary>
public class AccountRelationsValidator : AbstractValidator<Account>
{
    /// <summary>
    /// Configure validation rules.
    /// </summary>
    /// <param name="currencyRepository"></param>
    public AccountRelationsValidator(IEntityRepository<Currency> currencyRepository)
    {
        RuleFor(x => x.CurrencyId)
            .MustAsync(async (currencyId, cancellationToken) =>
            {
                var entity = await currencyRepository.GetByIdReadonlyAsync(currencyId, cancellationToken);

                return entity != null;
            })
            .WithMessage((account, id) => $"Currency with id={account.CurrencyId} not found");
    }
}