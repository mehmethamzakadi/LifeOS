using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Roles.Queries.GetRoleById;

public sealed record GetRoleByIdRequest(Guid Id) : IRequest<IDataResult<GetRoleByIdQueryResponse>>;
