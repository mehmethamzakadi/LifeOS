using LifeOS.Application.Features.MovieSeries.Commands.Create;
using LifeOS.Application.Features.MovieSeries.Commands.Delete;
using LifeOS.Application.Features.MovieSeries.Commands.Update;
using LifeOS.Application.Features.MovieSeries.Queries.GetById;
using LifeOS.Application.Features.MovieSeries.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers;

public class MovieSeriesController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("search")]
    [HasPermission(Permissions.MovieSeriesViewAll)]
    public async Task<IActionResult> Search([FromBody] DataGridRequest dataGridRequest)
    {
        PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse> response = await Mediator.Send(new GetPaginatedListByDynamicMovieSeriesQuery(dataGridRequest));
        return Ok(response);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.MovieSeriesRead)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await Mediator.Send(new GetByIdMovieSeriesQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [HasPermission(Permissions.MovieSeriesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateMovieSeriesCommand command)
    {
        return ToResponse(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.MovieSeriesUpdate)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieSeriesCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        return ToResponse(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.MovieSeriesDelete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return ToResponse(await Mediator.Send(new DeleteMovieSeriesCommand(id)));
    }
}

