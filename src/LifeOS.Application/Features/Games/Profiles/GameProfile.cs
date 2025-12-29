using AutoMapper;
using LifeOS.Application.Features.Games.Commands.Create;
using LifeOS.Application.Features.Games.Commands.Delete;
using LifeOS.Application.Features.Games.Commands.Update;
using LifeOS.Application.Features.Games.Queries.GetById;
using LifeOS.Application.Features.Games.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Games.Profiles;

public sealed class GameProfile : Profile
{
    public GameProfile()
    {
        CreateMap<Game, CreateGameCommand>().ReverseMap();
        CreateMap<Game, UpdateGameCommand>().ReverseMap();
        CreateMap<Game, DeleteGameCommand>().ReverseMap();

        CreateMap<Game, GetPaginatedListByDynamicGamesResponse>().ReverseMap();
        CreateMap<Game, GetByIdGameResponse>().ReverseMap();
        CreateMap<Paginate<Game>, PaginatedListResponse<GetPaginatedListByDynamicGamesResponse>>().ReverseMap();
    }
}

