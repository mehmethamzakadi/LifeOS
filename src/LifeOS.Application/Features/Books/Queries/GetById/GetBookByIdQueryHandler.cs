using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.Books.Queries.GetById;

public sealed class GetBookByIdQueryHandler(
    IBookRepository bookRepository,
    ICacheService cacheService) : IRequestHandler<GetByIdBookQuery, IDataResult<GetByIdBookResponse>>
{
    public async Task<IDataResult<GetByIdBookResponse>> Handle(GetByIdBookQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Book(request.Id);
        var cacheValue = await cacheService.Get<GetByIdBookResponse>(cacheKey);
        if (cacheValue is not null)
            return new SuccessDataResult<GetByIdBookResponse>(cacheValue);

        var book = await bookRepository.GetByIdAsync(request.Id, cancellationToken);

        if (book is null)
            return new ErrorDataResult<GetByIdBookResponse>("Kitap bilgisi bulunamadı.");

        var response = new GetByIdBookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.CoverUrl,
            book.TotalPages,
            book.CurrentPage,
            book.Status,
            book.Rating,
            book.StartDate,
            book.EndDate);

        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.Book),
            null);

        return new SuccessDataResult<GetByIdBookResponse>(response);
    }
}

