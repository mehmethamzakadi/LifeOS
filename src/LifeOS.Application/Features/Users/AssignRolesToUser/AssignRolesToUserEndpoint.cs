using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.AssignRolesToUser;

public static class AssignRolesToUserEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/{id}/roles", async (
            Guid id,
            AssignRolesToUserCommand command,
            AssignRolesToUserHandler handler,
            IValidator<AssignRolesToUserCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(id, command, cancellationToken);
            return result.ToResult();
        })
        .WithName("AssignRolesToUser")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.RolesAssignPermissions)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

