using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.GetPersonalNoteById;

public sealed class GetPersonalNoteByIdHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public GetPersonalNoteByIdHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<GetPersonalNoteByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.PersonalNote(id);
        var cacheValue = await _cacheService.Get<GetPersonalNoteByIdResponse>(cacheKey);
        if (cacheValue is not null)
            return ApiResultExtensions.Success(cacheValue, "Kişisel not başarıyla getirildi");

        var personalNote = await _context.PersonalNotes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (personalNote is null)
            return ApiResultExtensions.Failure<GetPersonalNoteByIdResponse>("Kişisel not bulunamadı.");

        var response = new GetPersonalNoteByIdResponse(
            personalNote.Id,
            personalNote.Title,
            personalNote.Content,
            personalNote.Category,
            personalNote.IsPinned,
            personalNote.Tags);

        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNote),
            null);

        return ApiResultExtensions.Success(response, "Kişisel not başarıyla getirildi");
    }
}

