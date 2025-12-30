using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.DeletePersonalNote;

public sealed class DeletePersonalNoteHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeletePersonalNoteHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var personalNote = await _context.PersonalNotes
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        
        if (personalNote is null)
            return ApiResultExtensions.Failure(ResponseMessages.PersonalNote.NotFound);

        personalNote.Delete();
        _context.PersonalNotes.Update(personalNote);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Remove(CacheKeys.PersonalNote(personalNote.Id));

        await _cacheService.Add(
            CacheKeys.PersonalNoteGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.PersonalNote.Deleted);
    }
}

