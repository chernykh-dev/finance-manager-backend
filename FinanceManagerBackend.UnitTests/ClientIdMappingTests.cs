using FinanceManagerBackend.API.Configuration;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Accounts;
using FinanceManagerBackend.API.Models.Transactions;
using Mapster;

namespace FinanceManagerBackend.UnitTests;

/// <summary>
/// Create idempotency relies on the client id reaching the entity: BaseCreateRequest.Id -> BaseEntity.Id.
/// Mapster does it by name convention only, so pin it here.
/// </summary>
public class ClientIdMappingTests
{
    public ClientIdMappingTests()
    {
        MapsterConfiguration.RegisterMappings();
    }

    [Fact]
    public void WhenCreateTransactionRequestHasId_EntityShouldKeepIt()
    {
        var id = Guid.NewGuid();

        var entity = new CreateTransactionRequest { Id = id }.Adapt<Transaction>();

        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void WhenCreateAccountRequestHasId_EntityShouldKeepIt()
    {
        var id = Guid.NewGuid();

        var entity = new CreateAccountRequest { Id = id, Name = "Card" }.Adapt<Account>();

        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void WhenCreateRequestHasNoId_EntityIdShouldBeEmpty()
    {
        // Controllers generate a new id only for Guid.Empty.
        var entity = new CreateTransactionRequest { Id = null }.Adapt<Transaction>();

        Assert.Equal(Guid.Empty, entity.Id);
    }
}
