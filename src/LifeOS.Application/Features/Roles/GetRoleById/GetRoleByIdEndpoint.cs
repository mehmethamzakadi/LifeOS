using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Roles.GetRoleById;

public static class GetRoleByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/roles/{id}", async (
            Guid id,
            GetRoleByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetRoleById")
        .WithTags("Roles")
        .RequireAuthorization(Domain.Constants.Permissions.RolesRead)
        .Produces<ApiResult<GetRoleByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetRoleByIdResponse>>(StatusCodes.Status404NotFound);
    }
}

