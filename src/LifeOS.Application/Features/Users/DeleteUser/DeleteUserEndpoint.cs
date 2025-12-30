using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.DeleteUser;

public static class DeleteUserEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/users/{id}", async (
            Guid id,
            DeleteUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.UsersDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

