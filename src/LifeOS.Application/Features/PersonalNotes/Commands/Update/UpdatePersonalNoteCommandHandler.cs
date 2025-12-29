using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.PersonalNotes.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.PersonalNotes.Commands.Update;

public sealed class UpdatePersonalNoteCommandHandler(
    IPersonalNoteRepository personalNoteRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdatePersonalNoteCommand, IResult>
{
    public async Task<IResult> Handle(UpdatePersonalNoteCommand request, CancellationToken cancellationToken)
    {
        var personalNote = await personalNoteRepository.GetAsync(
            predicate: x => x.Id == request.Id,
            enableTracking: true,
            cancellationToken: cancellationToken);

        if (personalNote is null)
        {
            return new ErrorResult(ResponseMessages.PersonalNote.NotFound);
        }

        personalNote.Update(
            request.Title,
            request.Content,
            request.Category,
            request.IsPinned,
            request.Tags);

        personalNoteRepository.Update(personalNote);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Add(
            CacheKeys.PersonalNote(personalNote.Id),
            new GetByIdPersonalNoteResponse(
                personalNote.Id,
                personalNote.Title,
                personalNote.Content,
                personalNote.Category,
                personalNote.IsPinned,
                personalNote.Tags),
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNote),
            null);

        await cacheService.Add(
            CacheKeys.PersonalNoteGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.PersonalNote.Updated);
    }
}

