using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.Endpoints;

public static class DeletePersonalNote
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/personalnotes/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var personalNote = await context.PersonalNotes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
            if (personalNote is null)
                return Results.NotFound(new { Error = ResponseMessages.PersonalNote.NotFound });

            personalNote.Delete();
            context.PersonalNotes.Update(personalNote);
            await context.SaveChangesAsync(cancellationToken);

            await cacheService.Remove(CacheKeys.PersonalNote(personalNote.Id));

            await cacheService.Add(
                CacheKeys.PersonalNoteGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.NoContent();
        })
        .WithName("DeletePersonalNote")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesDelete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

