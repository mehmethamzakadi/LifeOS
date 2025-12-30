using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class UpdateUser
{
    public sealed record Request(
        Guid Id,
        string UserName,
        string Email);

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
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Kullanıcı ID'si gereklidir");

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
        app.MapPut("api/users/{id}", async (
            Guid id,
            Request request,
            LifeOSDbContext context,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            if (id != request.Id)
                return ApiResultExtensions.Failure("ID uyuşmazlığı").ToResult();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);
            if (user is null)
                return ApiResultExtensions.Failure(ResponseMessages.User.NotFound).ToResult();

            if (user.Email != request.Email)
            {
                var existingEmail = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
                if (existingEmail != null && existingEmail.Id != request.Id)
                    return ApiResultExtensions.Failure(ResponseMessages.User.EmailAlreadyExists).ToResult();
            }

            if (user.UserName != request.UserName)
            {
                var existingUserName = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);
                if (existingUserName != null && existingUserName.Id != request.Id)
                    return ApiResultExtensions.Failure(ResponseMessages.User.UsernameAlreadyExists).ToResult();
            }

            user.Update(request.UserName, request.Email);
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            return ApiResultExtensions.Success(ResponseMessages.User.Updated).ToResult();
        })
        .WithName("UpdateUser")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

