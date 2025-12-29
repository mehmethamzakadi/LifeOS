using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Users.Queries.GetUserRoles;

public record GetUserRolesQuery(Guid UserId) : IRequest<IDataResult<GetUserRolesResponse>>;
