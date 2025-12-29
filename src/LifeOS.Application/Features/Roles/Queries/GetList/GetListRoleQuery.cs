using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Roles.Queries.GetList;

public sealed record GetListRoleQuery(PaginatedRequest PageRequest) : IRequest<PaginatedListResponse<GetListRoleResponse>>;

