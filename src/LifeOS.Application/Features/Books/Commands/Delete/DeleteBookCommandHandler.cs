using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Books.Commands.Delete;

public sealed class DeleteBookCommandHandler(
    IBookRepository bookRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteBookCommand, IResult>
{
    public async Task<IResult> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetAsync(predicate: x => x.Id == request.Id, enableTracking: true, cancellationToken: cancellationToken);
        if (book is null)
            return new ErrorResult(ResponseMessages.Book.NotFound);

        book.Delete();
        bookRepository.Update(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Remove(CacheKeys.Book(book.Id));

        await cacheService.Add(
            CacheKeys.BookGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Book.Deleted);
    }
}

