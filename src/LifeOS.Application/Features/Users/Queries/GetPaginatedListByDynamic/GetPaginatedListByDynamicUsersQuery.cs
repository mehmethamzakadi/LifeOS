using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Users.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicUsersQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>>;
