using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Roles.Queries.GetList;

public sealed record GetListRoleQuery(PaginatedRequest PageRequest) : IRequest<PaginatedListResponse<GetListRoleResponse>>;

