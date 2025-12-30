using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.GetUserById;

public static class GetUserByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{id}", async (
            Guid id,
            GetUserByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetUserById")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.UsersRead)
        .Produces<ApiResult<GetUserByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetUserByIdResponse>>(StatusCodes.Status404NotFound);
    }
}

