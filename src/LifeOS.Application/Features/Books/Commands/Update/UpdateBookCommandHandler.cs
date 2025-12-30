using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Books.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Books.Commands.Update;

public sealed class UpdateBookCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookCommand, IResult>
{
    public async Task<IResult> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await context.Books
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

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

        context.Books.Update(book);
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

