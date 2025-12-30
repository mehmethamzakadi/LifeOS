using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.Endpoints;

public static class GetBookById
{
    public sealed record Response(
        Guid Id,
        string Title,
        string Author,
        string? CoverUrl,
        int TotalPages,
        int CurrentPage,
        BookStatus Status,
        int? Rating,
        DateTime? StartDate,
        DateTime? EndDate);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/books/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var cacheKey = CacheKeys.Book(id);
            var cacheValue = await cacheService.Get<Response>(cacheKey);
            if (cacheValue is not null)
                return Results.Ok(cacheValue);

            var book = await context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (book is null)
                return Results.NotFound(new { Error = "Kitap bilgisi bulunamadı." });

            var response = new Response(
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

            return Results.Ok(response);
        })
        .WithName("GetBookById")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}

