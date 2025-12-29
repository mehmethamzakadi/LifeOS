using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.MovieSeries.Queries.GetById;

public sealed record GetByIdMovieSeriesQuery(Guid Id) : IRequest<IDataResult<GetByIdMovieSeriesResponse>>;

