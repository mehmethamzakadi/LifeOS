using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class CreateRole
{
    public sealed record Request(string Name);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Rol adı gereklidir")
                .MinimumLength(3).WithMessage("Rol adı en az 3 karakter olmalıdır")
                .MaximumLength(100).WithMessage("Rol adı en fazla 100 karakter olabilir");
        }
    }

    public sealed record Response(Guid Id);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/roles", async (
            Request request,
            LifeOSDbContext context,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var normalizedName = request.Name.ToUpperInvariant();
            var checkRole = await context.Roles
                .AnyAsync(r => r.NormalizedName == normalizedName, cancellationToken);
            if (checkRole)
                return ApiResultExtensions.Failure<Response>(ResponseMessages.Role.AlreadyExists).ToResult();

            var role = Role.Create(request.Name);
            await context.Roles.AddAsync(role, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var response = new Response(role.Id);
            return ApiResultExtensions.CreatedResult(response, $"/api/roles/{role.Id}", ResponseMessages.Role.Created);
        })
        .WithName("CreateRole")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesCreate)
        .Produces<ApiResult<Response>>(StatusCodes.Status201Created)
        .Produces<ApiResult<Response>>(StatusCodes.Status400BadRequest);
    }
}

