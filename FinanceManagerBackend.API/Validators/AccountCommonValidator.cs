using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FluentValidation;

namespace FinanceManagerBackend.API.Validators;

public class AccountCommonValidator : AbstractValidator<Account>
{
    public AccountCommonValidator(IEntityRepository<Currency> currencyRepository)
    {
        RuleFor(x => x.CurrencyId)
            .MustAsync(async (currencyId, cancellationToken) =>
            {
                var entity = await currencyRepository.GetByIdReadonlyAsync(currencyId, cancellationToken);

                return entity != null;
            })
            .WithMessage("Currency not found");
    }
}