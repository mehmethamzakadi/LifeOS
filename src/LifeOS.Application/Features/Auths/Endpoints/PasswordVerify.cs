using LifeOS.Application.Abstractions.Identity;
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

    public sealed record Response(bool Success, bool IsValid, string? Message = null);

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
                return Results.BadRequest(new Response(false, false, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var result = await authService.PasswordVerify(request.ResetToken, request.UserId);
            return Results.Ok(new Response(result.Success, result.Data, result.Message));
        })
        .WithName("PasswordVerify")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces<Response>(StatusCodes.Status400BadRequest);
    }
}

