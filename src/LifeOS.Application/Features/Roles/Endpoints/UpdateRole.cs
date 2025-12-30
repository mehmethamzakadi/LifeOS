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
                return ApiResultExtensions.Failure("ID uyuşmazlığı").ToResult();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

            if (role == null)
                return ApiResultExtensions.Failure(ResponseMessages.Role.NotFound).ToResult();

            var normalizedName = request.Name.ToUpperInvariant();
            var existingRole = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
            if (existingRole != null && existingRole.Id != request.Id)
                return ApiResultExtensions.Failure(ResponseMessages.Role.AlreadyExistsWithName(request.Name)).ToResult();

            role.Update(request.Name);
            context.Roles.Update(role);
            await context.SaveChangesAsync(cancellationToken);

            return ApiResultExtensions.Success(ResponseMessages.Role.Updated).ToResult();
        })
        .WithName("UpdateRole")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

