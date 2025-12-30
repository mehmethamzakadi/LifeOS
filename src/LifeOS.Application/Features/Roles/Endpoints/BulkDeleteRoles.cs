using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class BulkDeleteRoles
{
    public sealed record Request(List<Guid> RoleIds);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.RoleIds)
                .NotNull().WithMessage("Rol ID listesi gereklidir")
                .NotEmpty().WithMessage("En az bir rol ID'si gereklidir");
        }
    }

    public sealed record Response(
        int DeletedCount,
        int FailedCount,
        List<string> Errors);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/roles/bulk-delete", async (
            Request request,
            LifeOSDbContext context,
            ICurrentUserService currentUserService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(validationErrors).ToResult();
            }

            var deletedCount = 0;
            var failedCount = 0;
            var errors = new List<string>();

            foreach (var roleId in request.RoleIds)
            {
                try
                {
                    var role = await context.Roles
                        .Include(r => r.UserRoles)
                        .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

                    if (role == null)
                    {
                        errors.Add($"Rol bulunamadı: ID {roleId}");
                        failedCount++;
                        continue;
                    }

                    if (role.NormalizedName == "ADMIN")
                    {
                        errors.Add($"Admin rolü silinemez");
                        failedCount++;
                        continue;
                    }

                    if (role.UserRoles.Any(ur => !ur.IsDeleted))
                    {
                        errors.Add($"'{role.Name}' rolüne atanmış aktif kullanıcılar bulunmaktadır");
                        failedCount++;
                        continue;
                    }

                    role.Delete();
                    context.Roles.Update(role);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Rol silinirken hata oluştu (ID {roleId}): {ex.Message}");
                    failedCount++;
                }
            }

            var response = new Response(deletedCount, failedCount, errors);

            if (deletedCount > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            var message = failedCount > 0 
                ? $"{deletedCount} rol silindi, {failedCount} rol silinemedi"
                : $"{deletedCount} rol başarıyla silindi";

            return ApiResultExtensions.Success(response, message).ToResult();
        })
        .WithName("BulkDeleteRoles")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesDelete)
        .Produces<ApiResult<Response>>(StatusCodes.Status200OK)
        .Produces<ApiResult<Response>>(StatusCodes.Status400BadRequest);
    }
}

