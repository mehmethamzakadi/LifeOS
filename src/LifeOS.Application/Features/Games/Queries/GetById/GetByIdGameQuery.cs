using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Games.Queries.GetById;

public sealed record GetByIdGameQuery(Guid Id) : IRequest<IDataResult<GetByIdGameResponse>>;

