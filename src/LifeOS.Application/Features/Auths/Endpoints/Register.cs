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

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class Register
{
    public sealed record Request(
        string UserName,
        string Email,
        string Password);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz!")
                .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır!")
                .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir!")
                .Matches("^[a-zA-Z0-9\\-._@]+$").WithMessage("Kullanıcı adı sadece harf, rakam ve -._@ karakterlerini içerebilir!");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi boş olamaz!")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin!");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz!")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır!")
                .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir!")
                .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir!")
                .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir!")
                .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir!")
                .Matches("[^a-zA-Z0-9]").WithMessage("Şifre en az bir özel karakter içermelidir!");
        }
    }

    public sealed record Response(bool Success, string Message);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register", async (
            Request request,
            LifeOSDbContext context,
            IUserDomainService userDomainService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new Response(false, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var existingUser = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (existingUser is not null)
            {
                return Results.BadRequest(new Response(false, "Bu e-posta adresi zaten kullanılıyor!"));
            }

            var user = User.Create(request.UserName, request.Email, string.Empty);

            var passwordResult = userDomainService.SetPassword(user, request.Password);
            if (!passwordResult.Success)
                return Results.BadRequest(new Response(false, passwordResult.Message ?? "Şifre ayarlanamadı"));

            await context.Users.AddAsync(user, cancellationToken);

            var userRole = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.NormalizedName.Equals(UserRoles.User, StringComparison.InvariantCultureIgnoreCase) && !r.IsDeleted, cancellationToken);
            if (userRole != null)
            {
                var roleResult = userDomainService.AddToRole(user, userRole);
                if (!roleResult.Success)
                    return Results.BadRequest(new Response(false, roleResult.Message ?? "Rol atanamadı"));
            }

            await context.SaveChangesAsync(cancellationToken);
            return Results.Ok(new Response(true, "Kayıt işlemi başarılı. Giriş yapabilirsiniz."));
        })
        .WithName("Register")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces<Response>(StatusCodes.Status400BadRequest);
    }
}

