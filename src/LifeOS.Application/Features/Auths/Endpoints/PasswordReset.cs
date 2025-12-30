using LifeOS.Application.Abstractions.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class PasswordReset
{
    public sealed record Request(string Email);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi gereklidir")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin!");
        }
    }

    public sealed record Response(bool Success, string Message);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/password-reset", async (
            Request request,
            IAuthService authService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new Response(false, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            await authService.PasswordResetAsync(request.Email);
            return Results.Ok(new Response(true, "Şifre yenileme işlemleri için mail gönderildi."));
        })
        .WithName("PasswordReset")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces<Response>(StatusCodes.Status400BadRequest);
    }
}

