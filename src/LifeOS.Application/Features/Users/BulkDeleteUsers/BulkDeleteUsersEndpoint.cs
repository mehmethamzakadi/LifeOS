using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.BulkDeleteUsers;

public static class BulkDeleteUsersEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/bulk-delete", async (
            BulkDeleteUsersCommand command,
            BulkDeleteUsersHandler handler,
            IValidator<BulkDeleteUsersCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(validationErrors).ToResult();
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToResult();
        })
        .WithName("BulkDeleteUsers")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.UsersDelete)
        .Produces<ApiResult<BulkDeleteUsersResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<BulkDeleteUsersResponse>>(StatusCodes.Status400BadRequest);
    }
}

