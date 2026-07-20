using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Domain.Entities.Enums;
using FluentValidation;

namespace FinanceManagerBackend.API.Validators;

/// <summary>
/// Validator for check account foreign keys.
/// </summary>
public class AccountValidator : AbstractValidator<Account>
{
    /// <summary>
    /// Configure validation rules.
    /// </summary>
    /// <param name="currencyRepository"></param>
    public AccountValidator(IEntityRepository<Currency> currencyRepository)
    {
        RuleFor(x => x.CurrencyId)
            .MustAsync(async (currencyId, cancellationToken) =>
            {
                var entity = await currencyRepository.GetByIdReadonlyAsync(currencyId, cancellationToken);

                return entity != null;
            })
            .WithMessage((account, id) => $"Currency with id={account.CurrencyId} not found");

        RuleFor(x => x.Name)
            .Must(x => x.Length < 50)
            .WithMessage("Name length must be less than 50");
    }
}