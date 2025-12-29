using LifeOS.Application.Features.WalletTransactions.Commands.Create;
using LifeOS.Application.Features.WalletTransactions.Commands.Delete;
using LifeOS.Application.Features.WalletTransactions.Commands.Update;
using LifeOS.Application.Features.WalletTransactions.Queries.GetById;
using LifeOS.Application.Features.WalletTransactions.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers;

public class WalletTransactionController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("search")]
    [HasPermission(Permissions.WalletTransactionsViewAll)]
    public async Task<IActionResult> Search([FromBody] DataGridRequest dataGridRequest)
    {
        PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse> response = await Mediator.Send(new GetPaginatedListByDynamicWalletTransactionsQuery(dataGridRequest));
        return Ok(response);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.WalletTransactionsRead)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new GetByIdWalletTransactionQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [HasPermission(Permissions.WalletTransactionsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateWalletTransactionCommand command)
    {
        return ToResponse(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.WalletTransactionsUpdate)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateWalletTransactionCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        return ToResponse(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.WalletTransactionsDelete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return ToResponse(await Mediator.Send(new DeleteWalletTransactionCommand(id)));
    }
}

