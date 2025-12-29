using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.MovieSeries.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicMovieSeriesQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse>>;

