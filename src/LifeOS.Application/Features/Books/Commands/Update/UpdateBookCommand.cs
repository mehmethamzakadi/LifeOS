using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Enums;
using MediatR;

namespace LifeOS.Application.Features.Books.Commands.Update;

public sealed record UpdateBookCommand(
    Guid Id,
    string Title,
    string Author,
    string? CoverUrl = null,
    int TotalPages = 0,
    int CurrentPage = 0,
    BookStatus Status = BookStatus.ToRead,
    int? Rating = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.Book(Id);
        yield return CacheKeys.BookGridVersion();
    }
}

