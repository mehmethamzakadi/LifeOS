using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.PersonalNotes.Commands.Update;

public sealed record UpdatePersonalNoteCommand(
    Guid Id,
    string Title,
    string Content,
    string? Category = null,
    bool IsPinned = false,
    string? Tags = null) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.PersonalNote(Id);
        yield return CacheKeys.PersonalNoteGridVersion();
    }
}

