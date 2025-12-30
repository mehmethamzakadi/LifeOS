using LifeOS.Application.Common;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Roles.GetListRoles;

public static class GetListRolesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/roles", async (
            GetListRolesHandler handler,
            CancellationToken cancellationToken,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10) =>
        {
            var pageRequest = new PaginatedRequest { PageIndex = pageIndex, PageSize = pageSize };
            var result = await handler.HandleAsync(pageRequest, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetListRoles")
        .WithTags("Roles")
        .RequireAuthorization(Domain.Constants.Permissions.RolesViewAll)
        .Produces<ApiResult<PaginatedListResponse<GetListRolesResponse>>>(StatusCodes.Status200OK);
    }
}

