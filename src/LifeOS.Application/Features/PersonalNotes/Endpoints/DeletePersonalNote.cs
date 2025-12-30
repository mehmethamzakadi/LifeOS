using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
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
                return ApiResultExtensions.Failure(ResponseMessages.PersonalNote.NotFound).ToResult();

            personalNote.Delete();
            context.PersonalNotes.Update(personalNote);
            await context.SaveChangesAsync(cancellationToken);

            await cacheService.Remove(CacheKeys.PersonalNote(personalNote.Id));

            await cacheService.Add(
                CacheKeys.PersonalNoteGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return ApiResultExtensions.Success(ResponseMessages.PersonalNote.Deleted).ToResult();
        })
        .WithName("DeletePersonalNote")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

