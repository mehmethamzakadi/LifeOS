using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.PersonalNotes.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicPersonalNotesQueryHandler(
    IPersonalNoteRepository personalNoteRepository,
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

        Paginate<PersonalNote> personalNotesDynamic = await personalNoteRepository.GetPaginatedListByDynamicAsync(
            dynamic: request.DataGridRequest.DynamicQuery,
            index: pagination.PageIndex,
            size: pagination.PageSize,
            include: null,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>>(personalNotesDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNoteGrid),
            null);

        return response;
    }
}

