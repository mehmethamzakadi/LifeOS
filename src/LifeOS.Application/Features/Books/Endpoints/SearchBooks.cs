using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.Endpoints;

public static class SearchBooks
{
    public sealed record Response : BaseEntityResponse
    {
        public string Title { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string? CoverUrl { get; init; }
        public int TotalPages { get; init; }
        public int CurrentPage { get; init; }
        public BookStatus Status { get; init; }
        public int? Rating { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/books/search", async (
            DataGridRequest request,
            LifeOSDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var pagination = request.PaginatedRequest;
            var versionKey = CacheKeys.BookGridVersion();
            var versionToken = await cacheService.Get<string>(versionKey);
            if (string.IsNullOrWhiteSpace(versionToken))
            {
                versionToken = Guid.NewGuid().ToString("N");
                await cacheService.Add(versionKey, versionToken, null, null);
            }

            var cacheKey = CacheKeys.BookGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
            var cachedResponse = await cacheService.Get<PaginatedListResponse<Response>>(cacheKey);
            if (cachedResponse is not null)
            {
                return Results.Ok(cachedResponse);
            }

            var query = context.Books.AsNoTracking().AsQueryable();
            query = query.ToDynamic(request.DynamicQuery);
            var booksDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(booksDynamic);
            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.BookGrid),
                null);

            return Results.Ok(response);
        })
        .WithName("SearchBooks")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksViewAll)
        .Produces<PaginatedListResponse<Response>>(StatusCodes.Status200OK);
    }
}

