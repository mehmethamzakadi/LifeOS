using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Security;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class UpdateProfile
{
    public sealed record Request(
        string UserName,
        string Email,
        string? PhoneNumber,
        string? ProfilePictureUrl);

    public sealed class Validator : AbstractValidator<Request>
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UserNameRegex = new(
            @"^[a-zA-Z0-9_-]{3,50}$",
            RegexOptions.Compiled);

        public Validator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Kullanıcı adı gereklidir")
                .Length(3, 50).WithMessage("Kullanıcı adı 3 ile 50 karakter arasında olmalıdır")
                .Matches(UserNameRegex).WithMessage("Kullanıcı adı sadece harf, rakam, alt çizgi veya tire içerebilir")
                .Must(NotContainWhitespace).WithMessage("Kullanıcı adı boşluk içeremez");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta gereklidir")
                .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir")
                .Matches(EmailRegex).WithMessage("Geçersiz e-posta formatı")
                .EmailAddress().WithMessage("Geçersiz e-posta adresi");
        }

        private static bool NotContainWhitespace(string value)
        {
            return !string.IsNullOrEmpty(value) && !value.Any(char.IsWhiteSpace);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/profile", async (
            Request request,
            LifeOSDbContext context,
            ICurrentUserService currentUserService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userId = currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);
            if (user == null)
            {
                return Results.NotFound(new { Error = "Kullanıcı bulunamadı." });
            }

            if (user.Email != request.Email)
            {
                var existingEmail = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                if (existingEmail != null && existingEmail.Id != userId.Value)
                {
                    return Results.BadRequest(new { Error = "Bu e-posta adresi zaten kullanılıyor!" });
                }
            }

            if (user.UserName != request.UserName)
            {
                var existingUserName = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);
                if (existingUserName != null && existingUserName.Id != userId.Value)
                {
                    return Results.BadRequest(new { Error = "Bu kullanıcı adı zaten kullanılıyor!" });
                }
            }

            user.Update(request.UserName, request.Email);
            user.UpdateProfile(request.PhoneNumber, request.ProfilePictureUrl);

            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("UpdateProfile")
        .WithTags("Profile")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}

