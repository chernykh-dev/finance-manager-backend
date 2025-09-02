using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FluentValidation;

namespace FinanceManagerBackend.API.Validators;

public class TransactionCommonValidator : AbstractValidator<Transaction>
{
    public TransactionCommonValidator(IEntityRepository<Account> accountRepository,
        IEntityRepository<Category> categoryRepository)
    {
        RuleFor(x => x.AccountId)
            .MustAsync(async (accountId, cancellationToken) =>
            {
                var entity = await accountRepository.GetByIdReadonlyAsync(accountId, cancellationToken);

                return entity != null;
            })
            .WithMessage("Account not found");

        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellationToken) =>
            {
                var entity = await categoryRepository.GetByIdReadonlyAsync(categoryId, cancellationToken);

                return entity != null;
            })
            .WithMessage("Category not found");
    }
}