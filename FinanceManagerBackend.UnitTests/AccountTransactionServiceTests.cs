using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Infrastructure.MemoryBased;
using FinanceManagerBackend.API.Services;
using FinanceManagerBackend.UnitTests.CommonEntities;

namespace FinanceManagerBackend.UnitTests;

public class AccountTransactionServiceTests : IDisposable
{
    private readonly Transaction _sampleAccountTransaction = new Transaction()
    {
        Id = Guid.NewGuid(),
        AccountId = Accounts.SampleAccount.Id,
        CategoryId = Categories.SampleCategory.Id,
        Amount = 100,
        DateTime = DateTimeOffset.UtcNow
    };

    private readonly Transaction _sampleAccountIncomeTransaction = new Transaction()
    {
        Id = Guid.NewGuid(),
        AccountId = Accounts.SampleAccount.Id,
        CategoryId = Categories.SampleIncomeCategory.Id,
        Amount = 100,
        DateTime = DateTimeOffset.UtcNow
    };

    private readonly Transaction _sampleAccount2Transaction = new Transaction()
    {
        Id = Guid.NewGuid(),
        AccountId = Accounts.SampleAccount2.Id,
        CategoryId = Categories.SampleCategory.Id,
        Amount = 100,
        DateTime = DateTimeOffset.UtcNow
    };

    private readonly Transaction _sampleAccount2IncomeTransaction = new Transaction()
    {
        Id = Guid.NewGuid(),
        AccountId = Accounts.SampleAccount2.Id,
        CategoryId = Categories.SampleIncomeCategory.Id,
        Amount = 100,
        DateTime = DateTimeOffset.UtcNow
    };

    private readonly EntityMemoryRepository<Account> _accountMemoryRepository = new ();
    private readonly EntityMemoryRepository<Category> _categoryMemoryRepository = new ();

    private readonly AccountTransactionService _accountTransactionService;


    public AccountTransactionServiceTests()
    {
        _accountTransactionService = new AccountTransactionService(_accountMemoryRepository, _categoryMemoryRepository);
    }

    public void Dispose()
    {
        _accountMemoryRepository.Dispose();
        _categoryMemoryRepository.Dispose();

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task WhenApplyingNotIncomeTransaction_AccountBalanceShouldChange()
    {
        // Arrange.
        await _accountMemoryRepository.CreateAsync(Accounts.SampleAccount);
        await _categoryMemoryRepository.CreateAsync(Categories.SampleCategory);


        // Act.
        await _accountTransactionService.UpdateAccountAmountAsync(_sampleAccountTransaction);


        // Assert.
        var updatedAccount = await _accountMemoryRepository.GetRequiredByIdReadonlyAsync(Accounts.SampleAccount.Id);

        Assert.Equal(-_sampleAccountTransaction.Amount, updatedAccount.Balance);


        // Cleanup.
        Dispose();
    }

    [Fact]
    public async Task WhenApplyingIncomeTransaction_AccountBalanceShouldChange()
    {
        // Arrange.
        await _accountMemoryRepository.CreateAsync(Accounts.SampleAccount);
        await _categoryMemoryRepository.CreateAsync(Categories.SampleIncomeCategory);


        // Act.
        await _accountTransactionService.UpdateAccountAmountAsync(_sampleAccountIncomeTransaction);


        // Assert.
        var updatedAccount = await _accountMemoryRepository.GetRequiredByIdReadonlyAsync(Accounts.SampleAccount.Id);

        Assert.Equal(_sampleAccountIncomeTransaction.Amount, updatedAccount.Balance);


        // Cleanup.
        Dispose();
    }

    [Fact]
    public async Task WhenRollbackTransaction_AccountBalanceShouldRollback()
    {
        // Arrange.
        await _accountMemoryRepository.CreateAsync(Accounts.SampleAccount);
        await _categoryMemoryRepository.CreateAsync(Categories.SampleCategory);

        await _accountTransactionService.UpdateAccountAmountAsync(_sampleAccountTransaction);


        // Act.
        await _accountTransactionService.RollbackAccountAmountAsync(_sampleAccountTransaction);


        // Assert.
        var updatedAccount = await _accountMemoryRepository.GetRequiredByIdReadonlyAsync(Accounts.SampleAccount.Id);

        Assert.Equal(0, updatedAccount.Balance);


        // Cleanup.
        Dispose();
    }

    [Fact]
    public async Task WhenRollbackIncomeTransaction_AccountBalanceShouldRollback()
    {
        // Arrange.
        await _accountMemoryRepository.CreateAsync(Accounts.SampleAccount);
        await _categoryMemoryRepository.CreateAsync(Categories.SampleIncomeCategory);

        await _accountTransactionService.UpdateAccountAmountAsync(_sampleAccountIncomeTransaction);


        // Act.
        await _accountTransactionService.RollbackAccountAmountAsync(_sampleAccountIncomeTransaction);


        // Assert.
        var updatedAccount = await _accountMemoryRepository.GetRequiredByIdReadonlyAsync(Accounts.SampleAccount.Id);

        Assert.Equal(0, updatedAccount.Balance);


        // Cleanup.
        Dispose();
    }

    [Fact]
    public async Task WhenTransactionUpdatedToAnotherAccount_AccountBalancesShouldChange()
    {
        // Arrange.
        await _accountMemoryRepository.CreateAsync(Accounts.SampleAccount);
        await _accountMemoryRepository.CreateAsync(Accounts.SampleAccount2);

        await _categoryMemoryRepository.CreateAsync(Categories.SampleCategory);
        await _categoryMemoryRepository.CreateAsync(Categories.SampleIncomeCategory);

        await _accountTransactionService.UpdateAccountAmountAsync(_sampleAccountTransaction);


        // Act.
        await _accountTransactionService.ReplaceAccountAmountAsync(_sampleAccountTransaction,
            _sampleAccount2Transaction);


        // Assert.
        var updatedAccount = await _accountMemoryRepository.GetRequiredByIdReadonlyAsync(Accounts.SampleAccount.Id);
        var updatedAccount2 = await _accountMemoryRepository.GetRequiredByIdReadonlyAsync(Accounts.SampleAccount2.Id);

        Assert.Equal(0, updatedAccount.Balance);
        Assert.Equal(-_sampleAccount2Transaction.Amount, updatedAccount2.Balance);


        // Cleanup.
        Dispose();
    }
}