using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.PersonalNotes.Queries.GetById;

public sealed class GetPersonalNoteByIdQueryHandler(
    IPersonalNoteRepository personalNoteRepository,
    ICacheService cacheService) : IRequestHandler<GetByIdPersonalNoteQuery, IDataResult<GetByIdPersonalNoteResponse>>
{
    public async Task<IDataResult<GetByIdPersonalNoteResponse>> Handle(GetByIdPersonalNoteQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.PersonalNote(request.Id);
        var cacheValue = await cacheService.Get<GetByIdPersonalNoteResponse>(cacheKey);
        if (cacheValue is not null)
            return new SuccessDataResult<GetByIdPersonalNoteResponse>(cacheValue);

        var personalNote = await personalNoteRepository.GetByIdAsync(request.Id, cancellationToken);

        if (personalNote is null)
            return new ErrorDataResult<GetByIdPersonalNoteResponse>("Kişisel not bulunamadı.");

        var response = new GetByIdPersonalNoteResponse(
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

        return new SuccessDataResult<GetByIdPersonalNoteResponse>(response);
    }
}

