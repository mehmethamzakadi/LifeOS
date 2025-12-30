using LifeOS.Application.Common.Security;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class UpdateRole
{
    public sealed record Request(Guid Id, string Name);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Rol ID'si gereklidir");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Rol adı gereklidir")
                .MinimumLength(3).WithMessage("Rol adı en az 3 karakter olmalıdır")
                .MaximumLength(100).WithMessage("Rol adı en fazla 100 karakter olabilir");
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/roles/{id}", async (
            Guid id,
            Request request,
            LifeOSDbContext context,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            if (id != request.Id)
                return Results.BadRequest(new { Error = "ID mismatch" });

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

            if (role == null)
                return Results.NotFound(new { Error = "Rol bulunamadı!" });

            var normalizedName = request.Name.ToUpperInvariant();
            var existingRole = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
            if (existingRole != null && existingRole.Id != request.Id)
                return Results.BadRequest(new { Error = $"Güncellemek istediğiniz {request.Name} rolü sistemde mevcut!" });

            role.Update(request.Name);
            context.Roles.Update(role);
            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("UpdateRole")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesUpdate)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

