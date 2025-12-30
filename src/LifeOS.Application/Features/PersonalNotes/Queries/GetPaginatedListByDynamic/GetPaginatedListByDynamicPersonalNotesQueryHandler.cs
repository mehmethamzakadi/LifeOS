using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicPersonalNotesQueryHandler(
    LifeOSDbContext context,
    IMapper mapper,
    ICacheService cacheService) : IRequestHandler<GetPaginatedListByDynamicPersonalNotesQuery, PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>> Handle(GetPaginatedListByDynamicPersonalNotesQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.DataGridRequest.PaginatedRequest;
        var versionKey = CacheKeys.PersonalNoteGridVersion();
        var versionToken = await cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.PersonalNoteGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DataGridRequest.DynamicQuery);
        var cachedResponse = await cacheService.Get<PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var query = context.PersonalNotes.AsNoTracking().AsQueryable();
        query = query.ToDynamic(request.DataGridRequest.DynamicQuery);
        var personalNotesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>>(personalNotesDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNoteGrid),
            null);

        return response;
    }
}

