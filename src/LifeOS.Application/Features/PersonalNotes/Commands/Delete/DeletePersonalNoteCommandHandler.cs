using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.PersonalNotes.Commands.Delete;

public sealed class DeletePersonalNoteCommandHandler(
    IPersonalNoteRepository personalNoteRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeletePersonalNoteCommand, IResult>
{
    public async Task<IResult> Handle(DeletePersonalNoteCommand request, CancellationToken cancellationToken)
    {
        var personalNote = await personalNoteRepository.GetAsync(predicate: x => x.Id == request.Id, enableTracking: true, cancellationToken: cancellationToken);
        if (personalNote is null)
            return new ErrorResult(ResponseMessages.PersonalNote.NotFound);

        personalNote.Delete();
        personalNoteRepository.Update(personalNote);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Remove(CacheKeys.PersonalNote(personalNote.Id));

        await cacheService.Add(
            CacheKeys.PersonalNoteGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.PersonalNote.Deleted);
    }
}

