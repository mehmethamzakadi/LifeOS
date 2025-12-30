using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Permissions.AssignPermissionsToRole;

public static class AssignPermissionsToRoleEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/permissions/role/{roleId}", async (
            Guid roleId,
            AssignPermissionsToRoleCommand command,
            AssignPermissionsToRoleHandler handler,
            IValidator<AssignPermissionsToRoleCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(roleId, command, cancellationToken);
            return result.ToResult();
        })
        .WithName("AssignPermissionsToRole")
        .WithTags("Permissions")
        .RequireAuthorization(Domain.Constants.Permissions.RolesAssignPermissions)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

