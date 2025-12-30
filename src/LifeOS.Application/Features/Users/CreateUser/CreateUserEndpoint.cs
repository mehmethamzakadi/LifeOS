using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.CreateUser;

public static class CreateUserEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users", async (
            CreateUserCommand command,
            CreateUserHandler handler,
            IValidator<CreateUserCommand> validator,
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
                    $"/api/users/{response.Id}",
                    ResponseMessages.User.Created);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<CreateUserResponse>(ex.Message).ToResult();
            }
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.UsersCreate)
        .Produces<ApiResult<CreateUserResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateUserResponse>>(StatusCodes.Status400BadRequest);
    }
}

