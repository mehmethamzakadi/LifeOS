using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.UpdateProfile;

public static class UpdateProfileEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/profile", async (
            UpdateProfileCommand command,
            UpdateProfileHandler handler,
            IValidator<UpdateProfileCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToResult();
        })
        .WithName("UpdateProfile")
        .WithTags("Profile")
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

