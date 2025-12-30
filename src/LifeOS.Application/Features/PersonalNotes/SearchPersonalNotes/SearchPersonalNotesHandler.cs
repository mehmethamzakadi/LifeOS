using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.PersonalNotes.SearchPersonalNotes;

public sealed class SearchPersonalNotesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public SearchPersonalNotesHandler(
        LifeOSDbContext context,
        IMapper mapper,
        ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<PaginatedListResponse<SearchPersonalNotesResponse>>> HandleAsync(
        DataGridRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = request.PaginatedRequest;
        var versionKey = CacheKeys.PersonalNoteGridVersion();
        var versionToken = await _cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await _cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.PersonalNoteGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
        var cachedResponse = await _cacheService.Get<PaginatedListResponse<SearchPersonalNotesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return ApiResultExtensions.Success(cachedResponse, "Kişisel notlar başarıyla getirildi");
        }

        var query = _context.PersonalNotes.AsNoTracking().AsQueryable();
        query = query.ToDynamic(request.DynamicQuery);
        var personalNotesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<SearchPersonalNotesResponse> response = _mapper.Map<PaginatedListResponse<SearchPersonalNotesResponse>>(personalNotesDynamic);
        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNoteGrid),
            null);

        return ApiResultExtensions.Success(response, "Kişisel notlar başarıyla getirildi");
    }
}

