using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Events.PermissionEvents;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.Endpoints;

public static class AssignPermissionsToRole
{
    public sealed record Request(
        Guid RoleId,
        List<Guid> PermissionIds);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Rol ID'si gereklidir");

            RuleFor(x => x.PermissionIds)
                .NotNull().WithMessage("Permission listesi gereklidir");
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/permissions/role/{roleId}", async (
            Guid roleId,
            Request request,
            LifeOSDbContext context,
            ICurrentUserService currentUserService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            if (roleId != request.RoleId)
                return ApiResultExtensions.Failure("ID uyuşmazlığı").ToResult();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId && !r.IsDeleted, cancellationToken);

            if (role == null)
                return ApiResultExtensions.Failure("Rol bulunamadı").ToResult();

            var permissionsEntities = await context.Permissions
                .AsNoTracking()
                .Where(p => request.PermissionIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync(cancellationToken);
            var permissions = permissionsEntities.Select(p => p.Name).ToList();

            var existingRolePermissions = await context.RolePermissions
                .Where(rp => rp.RoleId == request.RoleId)
                .ToListAsync(cancellationToken);

            var existingPermissionIds = existingRolePermissions
                .Select(rp => rp.PermissionId)
                .ToHashSet();

            var requestedPermissionIds = request.PermissionIds.ToHashSet();

            var permissionsToRemove = existingPermissionIds.Except(requestedPermissionIds).ToList();
            var permissionsToAdd = requestedPermissionIds.Except(existingPermissionIds).ToList();

            if (permissionsToRemove.Any())
            {
                var rolePermissionsToRemove = existingRolePermissions
                    .Where(rp => permissionsToRemove.Contains(rp.PermissionId))
                    .ToList();

                context.RolePermissions.RemoveRange(rolePermissionsToRemove);
            }

            if (permissionsToAdd.Any())
            {
                foreach (var permissionId in permissionsToAdd)
                {
                    var newRolePermission = new RolePermission
                    {
                        RoleId = request.RoleId,
                        PermissionId = permissionId
                    };
                    await context.RolePermissions.AddAsync(newRolePermission, cancellationToken);
                }
            }

            role.AddDomainEvent(new PermissionsAssignedToRoleEvent(role.Id, role.Name!, permissions));

            await context.SaveChangesAsync(cancellationToken);

            return ApiResultExtensions.Success(ResponseMessages.Permission.Assigned).ToResult();
        })
        .WithName("AssignPermissionsToRole")
        .WithTags("Permissions")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesAssignPermissions)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

