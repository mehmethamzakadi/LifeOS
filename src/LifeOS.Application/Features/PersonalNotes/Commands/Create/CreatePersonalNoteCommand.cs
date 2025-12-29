using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.PersonalNotes.Commands.Create;

public sealed record CreatePersonalNoteCommand(
    string Title,
    string Content,
    string? Category = null,
    bool IsPinned = false,
    string? Tags = null) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.PersonalNoteGridVersion();
    }
}

