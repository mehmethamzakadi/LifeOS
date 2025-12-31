using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WatchPlatforms.GetAllWatchPlatforms;

public sealed class GetAllWatchPlatformsHandler
{
    private readonly LifeOSDbContext _context;

    public GetAllWatchPlatformsHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<List<GetAllWatchPlatformsResponse>>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var platforms = await _context.WatchPlatforms
            .Where(p => !p.IsDeleted)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var response = platforms.Select(p => new GetAllWatchPlatformsResponse(p.Id, p.Name)).ToList();

        return ApiResultExtensions.Success(response, "İzleme platformları başarıyla getirildi");
    }
}

