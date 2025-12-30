using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class PasswordVerify
{
    public sealed record Request(string ResetToken, string UserId);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ResetToken)
                .NotEmpty().WithMessage("Reset token gereklidir");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID'si gereklidir");
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/password-verify", async (
            Request request,
            IAuthService authService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await authService.PasswordVerify(request.ResetToken, request.UserId);
            var responseData = new { IsValid = result.Data, Message = result.Message };
            return ApiResultExtensions.Success(responseData, result.Message ?? "Token doğrulandı").ToResult();
        })
        .WithName("PasswordVerify")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}

