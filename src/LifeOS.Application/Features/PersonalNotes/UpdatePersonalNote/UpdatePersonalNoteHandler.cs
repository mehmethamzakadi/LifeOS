using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.PersonalNotes.GetPersonalNoteById;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.UpdatePersonalNote;

public sealed class UpdatePersonalNoteHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdatePersonalNoteHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdatePersonalNoteCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var personalNote = await _context.PersonalNotes
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);

        if (personalNote is null)
        {
            return ApiResultExtensions.Failure(ResponseMessages.PersonalNote.NotFound);
        }

        personalNote.Update(
            command.Title,
            command.Content,
            command.Category,
            command.IsPinned,
            command.Tags);

        _context.PersonalNotes.Update(personalNote);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.PersonalNote(personalNote.Id),
            new GetPersonalNoteByIdResponse(
                personalNote.Id,
                personalNote.Title,
                personalNote.Content,
                personalNote.Category,
                personalNote.IsPinned,
                personalNote.Tags),
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNote),
            null);

        await _cache.Add(
            CacheKeys.PersonalNoteGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.PersonalNote.Updated);
    }
}

