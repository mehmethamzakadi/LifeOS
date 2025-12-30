using LifeOS.Application.Abstractions;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class AssignRolesToUser
{
    public sealed record Request(
        Guid UserId,
        List<Guid> RoleIds);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID'si gereklidir");

            RuleFor(x => x.RoleIds)
                .NotNull().WithMessage("Rol listesi gereklidir");
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/{id}/roles", async (
            Guid id,
            Request request,
            LifeOSDbContext context,
            ICurrentUserService currentUserService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            if (id != request.UserId)
                return Results.BadRequest(new { Error = "ID mismatch" });

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);
            if (user == null)
                return Results.NotFound(new { Error = "Kullanıcı bulunamadı" });

            var requestedRoleIds = request.RoleIds.ToHashSet();

            var existingUserRoles = await context.UserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            var existingRoleIds = existingUserRoles
                .Where(ur => !ur.IsDeleted)
                .Select(ur => ur.RoleId)
                .ToHashSet();

            var rolesToRemove = existingRoleIds.Except(requestedRoleIds).ToList();
            var rolesToAdd = requestedRoleIds.Except(existingRoleIds).ToList();

            if (!rolesToRemove.Any() && !rolesToAdd.Any())
            {
                return Results.Ok(new { Message = "Roller zaten güncel" });
            }

            if (rolesToRemove.Any())
            {
                var userRolesToRemove = existingUserRoles
                    .Where(ur => rolesToRemove.Contains(ur.RoleId) && !ur.IsDeleted)
                    .ToList();

                foreach (var userRole in userRolesToRemove)
                {
                    userRole.Delete();
                    context.UserRoles.Update(userRole);
                }
            }

            if (rolesToAdd.Any())
            {
                foreach (var roleId in rolesToAdd)
                {
                    var deletedUserRole = existingUserRoles
                        .FirstOrDefault(ur => ur.RoleId == roleId && ur.IsDeleted);

                    if (deletedUserRole != null)
                    {
                        deletedUserRole.Restore();
                        context.UserRoles.Update(deletedUserRole);
                    }
                    else
                    {
                        var newUserRole = new UserRole
                        {
                            UserId = request.UserId,
                            RoleId = roleId
                        };
                        await context.UserRoles.AddAsync(newUserRole, cancellationToken);
                    }
                }
            }

            var currentRoles = await context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == request.UserId && !ur.IsDeleted)
                .Select(ur => ur.Role.Name!)
                .ToListAsync(cancellationToken);

            user.AddDomainEvent(new UserRolesAssignedEvent(user.Id, user.UserName, currentRoles));

            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { Message = "Roller başarıyla atandı" });
        })
        .WithName("AssignRolesToUser")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesAssignPermissions)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

