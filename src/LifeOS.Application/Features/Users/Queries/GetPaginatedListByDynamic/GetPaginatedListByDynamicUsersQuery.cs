using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Users.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicUsersQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>>;
