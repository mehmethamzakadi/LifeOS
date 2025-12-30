using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class ChangePassword
{
    public sealed record Request(
        string CurrentPassword,
        string NewPassword,
        string ConfirmPassword);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mevcut şifre gereklidir");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifre gereklidir")
                .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır")
                .MaximumLength(100).WithMessage("Yeni şifre en fazla 100 karakter olabilir")
                .Matches("[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir!")
                .Matches("[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir!")
                .Matches("[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir!")
                .Matches("[^a-zA-Z0-9]").WithMessage("Yeni şifre en az bir özel karakter içermelidir!");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Şifre onayı gereklidir")
                .Equal(x => x.NewPassword).WithMessage("Yeni şifre ve şifre onayı eşleşmiyor");
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/profile/change-password", async (
            Request request,
            LifeOSDbContext context,
            ICurrentUserService currentUserService,
            IUserDomainService userDomainService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var userId = currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                return ApiResultExtensions.Failure("Yetkisiz erişim").ToResult();
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);
            if (user == null)
            {
                return ApiResultExtensions.Failure("Kullanıcı bulunamadı.").ToResult();
            }

            if (!userDomainService.VerifyPassword(user, request.CurrentPassword))
            {
                return ApiResultExtensions.Failure("Mevcut şifre hatalı.").ToResult();
            }

            var result = userDomainService.SetPassword(user, request.NewPassword);
            if (!result.Success)
            {
                return ApiResultExtensions.Failure(result.Message ?? "Şifre ayarlanamadı").ToResult();
            }

            await context.SaveChangesAsync(cancellationToken);

            return ApiResultExtensions.Success("Şifre başarıyla değiştirildi.").ToResult();
        })
        .WithName("ChangePassword")
        .WithTags("Profile")
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

