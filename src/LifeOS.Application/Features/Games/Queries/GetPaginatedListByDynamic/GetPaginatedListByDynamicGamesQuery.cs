using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Games.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicGamesQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicGamesResponse>>;

