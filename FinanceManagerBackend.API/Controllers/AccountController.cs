﻿using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Accounts;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

public class AccountController(IEntityRepository<Account> accountRepository, IValidator<Account> accountCommonValidator)
    : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IList<AccountResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entities = await accountRepository.GetAllByReadonlyAsync(x => x.UserId == userId, cancellationToken);

        return Ok(entities.Adapt<List<AccountResponse>>());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await accountRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        return Ok(entity.Adapt<AccountResponse>());
    }

    [HttpPost]
    public async Task<ActionResult<AccountResponse>> CreateAsync([FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entity = request.Adapt<Account>();

        await accountCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

        var entityWithName =
            await accountRepository.GetByReadonlyAsync(x => x.Name == entity.Name && x.UserId == userId,
                cancellationToken);
        if (entityWithName != null)
        {
            return Conflict();
        }

        entity.Id = Guid.NewGuid();
        await accountRepository.CreateAsync(entity, cancellationToken);

        var result = CreatedAtAction("Get", new { id = entity.Id }, entity.Adapt<AccountResponse>());

        return result;
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await accountRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        var updatedEntity = request.Adapt<Account>();
        updatedEntity.Id = id;

        await accountCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

        await accountRepository.UpdateAsync(updatedEntity, cancellationToken);

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await accountRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        await accountRepository.DeleteAsync(entity, cancellationToken);

        return Ok();
    }
}