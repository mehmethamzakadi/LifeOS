using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.Dashboards.Queries.GetStatistics;

/// <summary>
/// Handler for getting dashboard statistics
/// </summary>
public sealed class GetStatisticsQueryHandler(
    ICategoryRepository categoryRepository,
    IUserRepository userRepository,
    IRoleRepository roleRepository)
    : IRequestHandler<GetStatisticsQuery, GetStatisticsResponse>
{
    public async Task<GetStatisticsResponse> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        var totalCategories = await categoryRepository.CountAsync(cancellationToken);
        var totalUsers = await userRepository.CountAsync(cancellationToken);
        var totalRoles = await roleRepository.CountAsync(cancellationToken);

        return new GetStatisticsResponse
        {
            TotalCategories = totalCategories,
            TotalUsers = totalUsers,
            TotalRoles = totalRoles
        };
    }
}
