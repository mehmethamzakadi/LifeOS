using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Categories.Queries.GetById;

public sealed record GetByIdCategoryQuery(Guid Id) : IRequest<IDataResult<GetByIdCategoryResponse>>;
