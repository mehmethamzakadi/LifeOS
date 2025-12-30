using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Roles.BulkDeleteRoles;

public static class BulkDeleteRolesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/roles/bulk-delete", async (
            BulkDeleteRolesCommand command,
            BulkDeleteRolesHandler handler,
            IValidator<BulkDeleteRolesCommand> validator,
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
        .WithName("BulkDeleteRoles")
        .WithTags("Roles")
        .RequireAuthorization(Domain.Constants.Permissions.RolesDelete)
        .Produces<ApiResult<BulkDeleteRolesResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<BulkDeleteRolesResponse>>(StatusCodes.Status400BadRequest);
    }
}

