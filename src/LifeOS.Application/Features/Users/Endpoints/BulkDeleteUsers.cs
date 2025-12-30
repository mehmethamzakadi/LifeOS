using LifeOS.Application.Abstractions;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class BulkDeleteUsers
{
    public sealed record Request(List<Guid> UserIds);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserIds)
                .NotNull().WithMessage("Kullanıcı ID listesi gereklidir")
                .NotEmpty().WithMessage("En az bir kullanıcı ID'si gereklidir");
        }
    }

    public sealed record Response(
        int DeletedCount,
        int FailedCount,
        List<string> Errors);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/bulk-delete", async (
            Request request,
            LifeOSDbContext context,
            ICurrentUserService currentUserService,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var deletedCount = 0;
            var failedCount = 0;
            var errors = new List<string>();

            foreach (var userId in request.UserIds)
            {
                try
                {
                    var user = await context.Users
                        .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                    if (user == null)
                    {
                        errors.Add($"Kullanıcı bulunamadı: ID {userId}");
                        failedCount++;
                        continue;
                    }

                    user.Delete();
                    context.Users.Update(user);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Kullanıcı silinirken hata oluştu (ID {userId}): {ex.Message}");
                    failedCount++;
                }
            }

            var response = new Response(deletedCount, failedCount, errors);

            if (deletedCount > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            return Results.Ok(response);
        })
        .WithName("BulkDeleteUsers")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersDelete)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

