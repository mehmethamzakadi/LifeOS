using LifeOS.Application.Features.ActivityLogs.Queries.GetPaginatedList;
using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeOS.API.Controllers;

public class ActivityLogsController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>
    /// Activity log'ları paginated ve filtrelenmiş şekilde getirir
    /// </summary>
    [HttpPost("search")]
    [HasPermission(Permissions.ActivityLogsView)]
    public async Task<IActionResult> GetPaginatedList([FromBody] DataGridRequest request)
    {
        PaginatedListResponse<GetPaginatedActivityLogsResponse> response =
            await Mediator.Send(new GetPaginatedActivityLogsQuery(request));
        return Ok(response);
    }
}
