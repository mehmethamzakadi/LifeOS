using AutoMapper;
using LifeOS.Application.Features.MovieSeries.Commands.Create;
using LifeOS.Application.Features.MovieSeries.Commands.Delete;
using LifeOS.Application.Features.MovieSeries.Commands.Update;
using LifeOS.Application.Features.MovieSeries.Queries.GetById;
using LifeOS.Application.Features.MovieSeries.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using MovieSeriesEntity = LifeOS.Domain.Entities.MovieSeries;

namespace LifeOS.Application.Features.MovieSeries.Profiles;

public sealed class MovieSeriesProfile : Profile
{
    public MovieSeriesProfile()
    {
        CreateMap<MovieSeriesEntity, CreateMovieSeriesCommand>().ReverseMap();
        CreateMap<MovieSeriesEntity, UpdateMovieSeriesCommand>().ReverseMap();
        CreateMap<MovieSeriesEntity, DeleteMovieSeriesCommand>().ReverseMap();

        CreateMap<MovieSeriesEntity, GetPaginatedListByDynamicMovieSeriesResponse>().ReverseMap();
        CreateMap<MovieSeriesEntity, GetByIdMovieSeriesResponse>().ReverseMap();
        CreateMap<Paginate<MovieSeriesEntity>, PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse>>().ReverseMap();
    }
}

