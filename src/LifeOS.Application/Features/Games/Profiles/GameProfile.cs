using AutoMapper;
using LifeOS.Application.Features.Games.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Games.Profiles;

public sealed class GameProfile : Profile
{
    public GameProfile()
    {
        CreateMap<Game, SearchGames.Response>().ReverseMap();
        CreateMap<Paginate<Game>, PaginatedListResponse<SearchGames.Response>>().ReverseMap();
    }
}

