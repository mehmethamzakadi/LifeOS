using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.SearchMovieSeries;

public sealed class SearchMovieSeriesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public SearchMovieSeriesHandler(
        LifeOSDbContext context,
        IMapper mapper,
        ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<PaginatedListResponse<SearchMovieSeriesResponse>>> HandleAsync(
        DataGridRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = request.PaginatedRequest;
        var versionKey = CacheKeys.MovieSeriesGridVersion();
        var versionToken = await _cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await _cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.MovieSeriesGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
        var cachedResponse = await _cacheService.Get<PaginatedListResponse<SearchMovieSeriesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return ApiResultExtensions.Success(cachedResponse, "Film/Diziler başarıyla getirildi");
        }

        var query = _context.MovieSeries
            .Include(m => m.Genre)
            .Include(m => m.WatchPlatform)
            .AsNoTracking()
            .AsQueryable();
        query = query.ToDynamic(request.DynamicQuery);
        var movieSeriesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        // Manual mapping because AutoMapper can't handle navigation properties easily
        var responseItems = movieSeriesDynamic.Items.Select(m => new SearchMovieSeriesResponse
        {
            Id = m.Id,
            Title = m.Title,
            CoverUrl = m.CoverUrl,
            MovieSeriesGenreId = m.MovieSeriesGenreId,
            GenreName = m.Genre.Name,
            WatchPlatformId = m.WatchPlatformId,
            WatchPlatformName = m.WatchPlatform.Name,
            CurrentSeason = m.CurrentSeason,
            CurrentEpisode = m.CurrentEpisode,
            Status = m.Status,
            Rating = m.Rating,
            PersonalNote = m.PersonalNote,
            CreatedDate = m.CreatedDate
        }).ToList();

        var response = new PaginatedListResponse<SearchMovieSeriesResponse>
        {
            Items = responseItems,
            Index = movieSeriesDynamic.Index,
            Size = movieSeriesDynamic.Size,
            Count = movieSeriesDynamic.Count,
            Pages = movieSeriesDynamic.Pages
        };
        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeriesGrid),
            null);

        return ApiResultExtensions.Success(response, "Film/Diziler başarıyla getirildi");
    }
}

