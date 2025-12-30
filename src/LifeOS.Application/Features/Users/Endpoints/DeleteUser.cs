using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class DeleteUser
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/users/{id}", async (
            Guid id,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

            if (user == null)
                return ApiResultExtensions.Failure(ResponseMessages.User.NotFound).ToResult();

            user.Delete();
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            return ApiResultExtensions.Success(ResponseMessages.User.Deleted).ToResult();
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

