using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GamePlatforms.GetAllGamePlatforms;

public sealed class GetAllGamePlatformsHandler
{
    private readonly LifeOSDbContext _context;

    public GetAllGamePlatformsHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<List<GetAllGamePlatformsResponse>>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var platforms = await _context.GamePlatforms
            .Where(p => !p.IsDeleted)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var response = platforms.Select(p => new GetAllGamePlatformsResponse(p.Id, p.Name)).ToList();

        return ApiResultExtensions.Success(response, "Oyun platformları başarıyla getirildi");
    }
}

