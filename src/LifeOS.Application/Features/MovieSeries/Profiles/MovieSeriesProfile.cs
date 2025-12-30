using AutoMapper;
using LifeOS.Application.Features.MovieSeries.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using MovieSeriesEntity = LifeOS.Domain.Entities.MovieSeries;

namespace LifeOS.Application.Features.MovieSeries.Profiles;

public sealed class MovieSeriesProfile : Profile
{
    public MovieSeriesProfile()
    {
        CreateMap<MovieSeriesEntity, SearchMovieSeries.Response>().ReverseMap();
        CreateMap<Paginate<MovieSeriesEntity>, PaginatedListResponse<SearchMovieSeries.Response>>().ReverseMap();
    }
}

