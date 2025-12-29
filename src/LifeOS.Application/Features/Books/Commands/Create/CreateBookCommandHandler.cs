using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Books.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Books.Commands.Create;

public sealed class CreateBookCommandHandler(
    IBookRepository bookRepository,
    ICacheService cache,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBookCommand, IResult>
{
    public async Task<IResult> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = Book.Create(
            request.Title,
            request.Author,
            request.CoverUrl,
            request.TotalPages,
            request.CurrentPage,
            request.Status,
            request.Rating,
            request.StartDate,
            request.EndDate);

        await bookRepository.AddAsync(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.Add(
            CacheKeys.Book(book.Id),
            new GetByIdBookResponse(
                Id: book.Id,
                Title: book.Title,
                Author: book.Author,
                CoverUrl: book.CoverUrl,
                TotalPages: book.TotalPages,
                CurrentPage: book.CurrentPage,
                Status: book.Status,
                Rating: book.Rating,
                StartDate: book.StartDate,
                EndDate: book.EndDate),
            DateTimeOffset.UtcNow.Add(CacheDurations.Book),
            null);

        await cache.Add(
            CacheKeys.BookGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Book.Created);
    }
}

