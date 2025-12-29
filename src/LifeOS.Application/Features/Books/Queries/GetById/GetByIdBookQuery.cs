using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Books.Queries.GetById;

public sealed record GetByIdBookQuery(Guid Id) : IRequest<IDataResult<GetByIdBookResponse>>;

