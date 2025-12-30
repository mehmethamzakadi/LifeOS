using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class DeleteRole
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/roles/{id}", async (
            Guid id,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var role = await context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

            if (role == null)
                return ApiResultExtensions.Failure(ResponseMessages.Role.NotFound).ToResult();

            if (role.NormalizedName == "ADMIN")
                return ApiResultExtensions.Failure("Admin rolü silinemez!").ToResult();

            if (role.UserRoles.Any(ur => !ur.IsDeleted))
                return ApiResultExtensions.Failure("Bu role atanmış aktif kullanıcılar bulunmaktadır. Önce kullanıcılardan bu rolü kaldırmalısınız.").ToResult();

            role.Delete();
            context.Roles.Update(role);
            await context.SaveChangesAsync(cancellationToken);

            return ApiResultExtensions.Success(ResponseMessages.Role.Deleted).ToResult();
        })
        .WithName("DeleteRole")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

