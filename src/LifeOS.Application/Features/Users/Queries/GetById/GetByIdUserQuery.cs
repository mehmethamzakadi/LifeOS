using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Users.Queries.GetById;

public sealed record GetByIdUserQuery(Guid Id) : IRequest<IDataResult<GetByIdUserResponse>>;
