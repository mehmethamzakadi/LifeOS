using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Games.SearchGames;

/// <summary>
/// AutoMapper profile for SearchGames feature.
/// This mapping is specific to SearchGames slice and kept within the slice.
/// </summary>
public sealed class SearchGamesMapping : Profile
{
    public SearchGamesMapping()
    {
        CreateMap<Game, SearchGamesResponse>().ReverseMap();
        CreateMap<Paginate<Game>, PaginatedListResponse<SearchGamesResponse>>().ReverseMap();
    }
}

