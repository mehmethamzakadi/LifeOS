using LifeOS.Application.Features.Books.Commands.Create;
using LifeOS.Application.Features.Books.Commands.Delete;
using LifeOS.Application.Features.Books.Commands.Update;
using LifeOS.Application.Features.Books.Queries.GetById;
using LifeOS.Application.Features.Books.Queries.GetPaginatedListByDynamic;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers;

public class BookController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("search")]
    [HasPermission(Permissions.BooksViewAll)]
    public async Task<IActionResult> Search([FromBody] DataGridRequest dataGridRequest)
    {
        PaginatedListResponse<GetPaginatedListByDynamicBooksResponse> response = await Mediator.Send(new GetPaginatedListByDynamicBooksQuery(dataGridRequest));
        return Ok(response);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.BooksRead)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new GetByIdBookQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [HasPermission(Permissions.BooksCreate)]
    public async Task<IActionResult> Create([FromBody] CreateBookCommand command)
    {
        return ToResponse(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.BooksUpdate)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateBookCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        return ToResponse(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.BooksDelete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return ToResponse(await Mediator.Send(new DeleteBookCommand(id)));
    }
}

