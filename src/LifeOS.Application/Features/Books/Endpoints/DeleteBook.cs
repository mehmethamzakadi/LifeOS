using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.Endpoints;

public static class DeleteBook
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/books/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cache,
            CancellationToken cancellationToken) =>
        {
            var book = await context.Books
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (book is null)
            {
                return Results.NotFound(new { Error = ResponseMessages.Book.NotFound });
            }

            book.Delete();
            context.Books.Update(book);
            await context.SaveChangesAsync(cancellationToken);

            // Cache invalidation
            await cache.Remove(CacheKeys.Book(book.Id));
            await cache.Add(
                CacheKeys.BookGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.NoContent();
        })
        .WithName("DeleteBook")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksDelete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

