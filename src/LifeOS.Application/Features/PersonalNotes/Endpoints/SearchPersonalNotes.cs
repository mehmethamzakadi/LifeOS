using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.Endpoints;

public static class SearchPersonalNotes
{
    public sealed record Response : BaseEntityResponse
    {
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public string? Category { get; init; }
        public bool IsPinned { get; init; }
        public string? Tags { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/personalnotes/search", async (
            DataGridRequest request,
            LifeOSDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var pagination = request.PaginatedRequest;
            var versionKey = CacheKeys.PersonalNoteGridVersion();
            var versionToken = await cacheService.Get<string>(versionKey);
            if (string.IsNullOrWhiteSpace(versionToken))
            {
                versionToken = Guid.NewGuid().ToString("N");
                await cacheService.Add(versionKey, versionToken, null, null);
            }

            var cacheKey = CacheKeys.PersonalNoteGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
            var cachedResponse = await cacheService.Get<PaginatedListResponse<Response>>(cacheKey);
            if (cachedResponse is not null)
            {
                return Results.Ok(cachedResponse);
            }

            var query = context.PersonalNotes.AsNoTracking().AsQueryable();
            query = query.ToDynamic(request.DynamicQuery);
            var personalNotesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(personalNotesDynamic);
            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNoteGrid),
                null);

            return Results.Ok(response);
        })
        .WithName("SearchPersonalNotes")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesViewAll)
        .Produces<PaginatedListResponse<Response>>(StatusCodes.Status200OK);
    }
}

