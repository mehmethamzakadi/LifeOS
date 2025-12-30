using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.Endpoints;

public static class GetPersonalNoteById
{
    public sealed record Response(
        Guid Id,
        string Title,
        string Content,
        string? Category,
        bool IsPinned,
        string? Tags);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/personalnotes/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var cacheKey = CacheKeys.PersonalNote(id);
            var cacheValue = await cacheService.Get<Response>(cacheKey);
            if (cacheValue is not null)
                return Results.Ok(cacheValue);

            var personalNote = await context.PersonalNotes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (personalNote is null)
                return Results.NotFound(new { Error = "Kişisel not bulunamadı." });

            var response = new Response(
                personalNote.Id,
                personalNote.Title,
                personalNote.Content,
                personalNote.Category,
                personalNote.IsPinned,
                personalNote.Tags);

            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNote),
                null);

            return Results.Ok(response);
        })
        .WithName("GetPersonalNoteById")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

