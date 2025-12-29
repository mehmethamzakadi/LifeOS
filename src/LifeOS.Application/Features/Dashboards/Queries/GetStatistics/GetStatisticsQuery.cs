using MediatR;

namespace LifeOS.Application.Features.Dashboards.Queries.GetStatistics;

public sealed record GetStatisticsQuery : IRequest<GetStatisticsResponse>;
