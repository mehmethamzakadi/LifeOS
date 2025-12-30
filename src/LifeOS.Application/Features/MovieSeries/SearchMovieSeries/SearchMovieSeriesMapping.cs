using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;
using MovieSeriesEntity = LifeOS.Domain.Entities.MovieSeries;

namespace LifeOS.Application.Features.MovieSeries.SearchMovieSeries;

/// <summary>
/// AutoMapper profile for SearchMovieSeries feature.
/// This mapping is specific to SearchMovieSeries slice and kept within the slice.
/// </summary>
public sealed class SearchMovieSeriesMapping : Profile
{
    public SearchMovieSeriesMapping()
    {
        CreateMap<MovieSeriesEntity, SearchMovieSeriesResponse>().ReverseMap();
        CreateMap<Paginate<MovieSeriesEntity>, PaginatedListResponse<SearchMovieSeriesResponse>>().ReverseMap();
    }
}

