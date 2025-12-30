using LifeOS.Application.Common.Security;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class CreateUser
{
    public sealed record Request(
        string UserName,
        string Email,
        string Password);

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

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre gereklidir")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
                .MaximumLength(128).WithMessage("Şifre en fazla 128 karakter olabilir")
                .Must(ContainUppercase).WithMessage("Şifre en az bir büyük harf içermelidir")
                .Must(ContainLowercase).WithMessage("Şifre en az bir küçük harf içermelidir")
                .Must(ContainDigit).WithMessage("Şifre en az bir rakam içermelidir")
                .Must(ContainSpecialCharacter).WithMessage("Şifre en az bir özel karakter içermelidir (!@#$%^&*()_+-=[]{}|;:,.<>?)");
        }

        private static bool NotContainWhitespace(string value)
        {
            return !string.IsNullOrEmpty(value) && !value.Any(char.IsWhiteSpace);
        }

        private static bool ContainUppercase(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
        }

        private static bool ContainLowercase(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
        }

        private static bool ContainDigit(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
        }

        private static bool ContainSpecialCharacter(string password)
        {
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            return !string.IsNullOrEmpty(password) && password.Any(c => specialChars.Contains(c));
        }
    }

    public sealed record Response(Guid Id);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users", async (
            Request request,
            LifeOSDbContext context,
            IUserDomainService userDomainService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var existingUser = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (existingUser is not null)
                return Results.BadRequest(new { Error = "Bu e-posta adresi zaten kullanılıyor!" });

            var existingUserName = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);
            if (existingUserName is not null)
                return Results.BadRequest(new { Error = "Bu kullanıcı adı zaten kullanılıyor!" });

            var user = User.Create(request.UserName, request.Email, string.Empty);

            var passwordResult = userDomainService.SetPassword(user, request.Password);
            if (!passwordResult.Success)
                return Results.BadRequest(new { Error = passwordResult.Message });

            await context.Users.AddAsync(user, cancellationToken);

            var userRole = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.NormalizedName == UserRoles.User.ToUpperInvariant(), cancellationToken);
            if (userRole != null)
            {
                var roleResult = userDomainService.AddToRole(user, userRole);
                if (!roleResult.Success)
                    return Results.BadRequest(new { Error = roleResult.Message });
            }

            await context.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/users/{user.Id}", new Response(user.Id));
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersCreate)
        .Produces<Response>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

