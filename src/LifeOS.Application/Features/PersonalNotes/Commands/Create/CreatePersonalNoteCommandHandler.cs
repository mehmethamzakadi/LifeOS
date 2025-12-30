using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.PersonalNotes.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.PersonalNotes.Commands.Create;

public sealed class CreatePersonalNoteCommandHandler(
    LifeOSDbContext context,
    ICacheService cache,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePersonalNoteCommand, IResult>
{
    public async Task<IResult> Handle(CreatePersonalNoteCommand request, CancellationToken cancellationToken)
    {
        var personalNote = PersonalNote.Create(
            request.Title,
            request.Content,
            request.Category,
            request.IsPinned,
            request.Tags);

        await context.PersonalNotes.AddAsync(personalNote, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.Add(
            CacheKeys.PersonalNote(personalNote.Id),
            new GetByIdPersonalNoteResponse(
                Id: personalNote.Id,
                Title: personalNote.Title,
                Content: personalNote.Content,
                Category: personalNote.Category,
                IsPinned: personalNote.IsPinned,
                Tags: personalNote.Tags),
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNote),
            null);

        await cache.Add(
            CacheKeys.PersonalNoteGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.PersonalNote.Created);
    }
}

