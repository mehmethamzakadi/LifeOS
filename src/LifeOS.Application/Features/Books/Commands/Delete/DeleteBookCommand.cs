using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Books.Commands.Delete;

public sealed record DeleteBookCommand(Guid Id) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.Book(Id);
        yield return CacheKeys.BookGridVersion();
    }
}

