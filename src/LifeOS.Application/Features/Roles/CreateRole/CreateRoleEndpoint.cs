using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Roles.CreateRole;

public static class CreateRoleEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/roles", async (
            CreateRoleCommand command,
            CreateRoleHandler handler,
            IValidator<CreateRoleCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            try
            {
                var response = await handler.HandleAsync(command, cancellationToken);
                return ApiResultExtensions.CreatedResult(
                    response,
                    $"/api/roles/{response.Id}",
                    ResponseMessages.Role.Created);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<CreateRoleResponse>(ex.Message).ToResult();
            }
        })
        .WithName("CreateRole")
        .WithTags("Roles")
        .RequireAuthorization(Domain.Constants.Permissions.RolesCreate)
        .Produces<ApiResult<CreateRoleResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateRoleResponse>>(StatusCodes.Status400BadRequest);
    }
}

