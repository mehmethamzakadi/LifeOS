using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Users.Queries.GetList;

public sealed record GetListUsersQuery(PaginatedRequest PageRequest) : IRequest<PaginatedListResponse<GetListUserResponse>>;
