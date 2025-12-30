using LifeOS.Application.Features.PersonalNotes.Commands.Create;
using LifeOS.Application.Features.PersonalNotes.Commands.Delete;
using LifeOS.Application.Features.PersonalNotes.Commands.Update;
using LifeOS.Application.Features.PersonalNotes.Queries.GetById;
using LifeOS.Application.Features.PersonalNotes.Queries.GetPaginatedListByDynamic;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers;

public class PersonalNoteController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("search")]
    [HasPermission(Permissions.PersonalNotesViewAll)]
    public async Task<IActionResult> Search([FromBody] DataGridRequest dataGridRequest)
    {
        PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse> response = await Mediator.Send(new GetPaginatedListByDynamicPersonalNotesQuery(dataGridRequest));
        return Ok(response);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.PersonalNotesRead)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new GetByIdPersonalNoteQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [HasPermission(Permissions.PersonalNotesCreate)]
    public async Task<IActionResult> Create([FromBody] CreatePersonalNoteCommand command)
    {
        return ToResponse(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.PersonalNotesUpdate)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePersonalNoteCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        return ToResponse(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.PersonalNotesDelete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return ToResponse(await Mediator.Send(new DeletePersonalNoteCommand(id)));
    }
}

