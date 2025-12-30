using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class Login
{
    public sealed record Request(
        string Email,
        string Password,
        string? DeviceId = null);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(u => u.Email).EmailAddress(EmailValidationMode.AspNetCoreCompatible).WithMessage("Email adresi geçersiz!");
        }
    }

    public sealed record Response(
        Guid UserId,
        string UserName,
        DateTime Expiration,
        string Token,
        List<string> Permissions);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (
            Request request,
            HttpContext httpContext,
            IAuthService authService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await authService.LoginAsync(request.Email, request.Password, request.DeviceId);

            if (!result.Success || result.Data is null)
            {
                AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
                return ApiResultExtensions.Failure<Response>("Giriş başarısız. E-posta veya şifre hatalı.").ToResult();
            }

            AuthCookieHelper.SetRefreshTokenCookie(httpContext, result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

            var responseData = new Response(
                result.Data.UserId,
                result.Data.UserName,
                result.Data.Expiration,
                result.Data.Token,
                result.Data.Permissions);

            return ApiResultExtensions.Success(responseData, "Giriş başarılı").ToResult();
        })
        .WithName("Login")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<ApiResult<Response>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

