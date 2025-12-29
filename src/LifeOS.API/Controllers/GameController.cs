using LifeOS.Application.Features.Games.Commands.Create;
using LifeOS.Application.Features.Games.Commands.Delete;
using LifeOS.Application.Features.Games.Commands.Update;
using LifeOS.Application.Features.Games.Queries.GetById;
using LifeOS.Application.Features.Games.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers;

public class GameController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("search")]
    [HasPermission(Permissions.GamesViewAll)]
    public async Task<IActionResult> Search([FromBody] DataGridRequest dataGridRequest)
    {
        PaginatedListResponse<GetPaginatedListByDynamicGamesResponse> response = await Mediator.Send(new GetPaginatedListByDynamicGamesQuery(dataGridRequest));
        return Ok(response);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.GamesRead)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new GetByIdGameQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [HasPermission(Permissions.GamesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateGameCommand command)
    {
        return ToResponse(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.GamesUpdate)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateGameCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        return ToResponse(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.GamesDelete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return ToResponse(await Mediator.Send(new DeleteGameCommand(id)));
    }
}

