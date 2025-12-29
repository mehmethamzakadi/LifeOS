using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Books.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Books.Commands.Update;

public sealed class UpdateBookCommandHandler(
    IBookRepository bookRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookCommand, IResult>
{
    public async Task<IResult> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetAsync(
            predicate: x => x.Id == request.Id,
            enableTracking: true,
            cancellationToken: cancellationToken);

        if (book is null)
        {
            return new ErrorResult(ResponseMessages.Book.NotFound);
        }

        book.Update(
            request.Title,
            request.Author,
            request.CoverUrl,
            request.TotalPages,
            request.CurrentPage,
            request.Status,
            request.Rating,
            request.StartDate,
            request.EndDate);

        bookRepository.Update(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Add(
            CacheKeys.Book(book.Id),
            new GetByIdBookResponse(
                book.Id,
                book.Title,
                book.Author,
                book.CoverUrl,
                book.TotalPages,
                book.CurrentPage,
                book.Status,
                book.Rating,
                book.StartDate,
                book.EndDate),
            DateTimeOffset.UtcNow.Add(CacheDurations.Book),
            null);

        await cacheService.Add(
            CacheKeys.BookGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Book.Updated);
    }
}

