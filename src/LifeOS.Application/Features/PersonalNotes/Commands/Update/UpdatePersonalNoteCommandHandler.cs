using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.PersonalNotes.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.PersonalNotes.Commands.Update;

public sealed class UpdatePersonalNoteCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdatePersonalNoteCommand, IResult>
{
    public async Task<IResult> Handle(UpdatePersonalNoteCommand request, CancellationToken cancellationToken)
    {
        var personalNote = await context.PersonalNotes
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

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

        context.PersonalNotes.Update(personalNote);
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

