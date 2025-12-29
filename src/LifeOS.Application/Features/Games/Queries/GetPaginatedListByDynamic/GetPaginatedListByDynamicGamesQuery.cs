using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Games.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicGamesQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicGamesResponse>>;

