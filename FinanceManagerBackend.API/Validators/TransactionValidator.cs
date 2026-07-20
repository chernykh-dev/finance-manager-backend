using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Domain.Entities.Enums;
using FluentValidation;

namespace FinanceManagerBackend.API.Validators;

/// <summary>
/// Validator for check transaction foreign keys.
/// </summary>
public class TransactionValidator : AbstractValidator<Transaction>
{
    /// <summary>
    /// Configure validation rules.
    /// </summary>
    /// <param name="accountRepository"></param>
    /// <param name="categoryRepository"></param>
    public TransactionValidator(IEntityRepository<Account> accountRepository,
        IEntityRepository<Category> categoryRepository)
    {
        RuleFor(x => x.AccountId)
            .MustAsync(async (accountId, cancellationToken) =>
            {
                var entity = await accountRepository.GetByIdReadonlyAsync(accountId, cancellationToken);

                return entity != null;
            })
            .WithMessage((transaction, id) => $"Account with id={transaction.AccountId} not found")
            .MustAsync(async (accountId, cancellationToken) =>
            {
                var entity = await accountRepository.GetByIdReadonlyAsync(accountId, cancellationToken);

                return entity != null && entity.Status != EStatus.Deleted;
            })
            .WithMessage((transaction, id) => $"Account with id={transaction.AccountId} is deleted");

        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellationToken) =>
            {
                var entity = await categoryRepository.GetByIdReadonlyAsync(categoryId, cancellationToken);

                return entity != null;
            })
            .WithMessage((transaction, id) => $"Category with id={transaction.CategoryId} not found");

        RuleFor(x => x.Amount)
            .NotEmpty()
            .Must(x => x > 0)
            .WithMessage("Amount must be positive");

        RuleFor(x => x.Comment)
            .Must(x => x!.Length < 200)
            .When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage("Comment length must be less than 200");
    }
}